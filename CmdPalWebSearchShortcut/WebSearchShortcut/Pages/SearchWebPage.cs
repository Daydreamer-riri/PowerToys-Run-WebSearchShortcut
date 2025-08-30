using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Commands;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.History;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Services;

namespace WebSearchShortcut;

internal sealed partial class SearchWebPage : DynamicListPage
{
    private const int MaxHistoryDisplayCount = 3;
    private const int MaxDisplayCount = 100;

    private readonly WebSearchShortcutDataEntry _shortcut;

    private readonly IListItem _openHomepageListItem;
    private readonly IContextItem _openHomepageContextItem;

    private IListItem[] _items = [];
    private IListItem[] _suggestionItems = [];

    private int _lastUpdateSearchTextEpoch;
    private readonly Lock _swapSuggestionsCancellationSourceLock = new();
    private readonly Lock _renderLock = new();
    private readonly Lock _updateSuggestionLock = new();
    private CancellationTokenSource? _previousSuggestionsCancellationSource;

    public SearchWebPage(WebSearchShortcutDataEntry shortcut)
    {
        Title = StringFormatter.Format(Resources.SearchWebPage_TitleTemplate, new() { ["shortcut"] = shortcut.Name });
        Name = $"[UNBOUND] {nameof(SearchWebPage)}.{nameof(Name)} required - shortcut='{shortcut.Name}'";
        Icon = IconService.GetIconInfo(shortcut);

        _shortcut = shortcut;

        var openHomepagecommand = new OpenHomePageCommand(shortcut)
        {
            Name = StringFormatter.Format(Resources.OpenHomepageItem_NameTemplate, new() { ["shortcut"] = shortcut.Name })
        };
        _openHomepageListItem = new ListItem(openHomepagecommand)
        {
            Title = StringFormatter.Format(Resources.OpenHomepageItem_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            Icon = Icons.Home
        };
        _openHomepageContextItem = new CommandContextItem(openHomepagecommand)
        {
            Title = StringFormatter.Format(Resources.OpenHomepageItem_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            Icon = Icons.Home
        };
    }

    public override IListItem[] GetItems()
    {
        if (_items.Length == 0)
            Rebuild();

        return Volatile.Read(ref _items);
    }

    public void Rebuild()
    {
        UpdateSearchText(SearchText, SearchText);
    }

    public override async void UpdateSearchText(string oldSearch, string newSearch)
    {
        int currentEpoch = Interlocked.Increment(ref _lastUpdateSearchTextEpoch);

        bool shouldOpenHomePage = string.IsNullOrEmpty(newSearch);
        bool shouldFetchSuggestions = !shouldOpenHomePage && !string.IsNullOrEmpty(_shortcut.SuggestionProvider);

        CancellationTokenSource? currentCancellationSource = shouldFetchSuggestions ? new CancellationTokenSource() : null;
        CancellationTokenSource? previousCancellationSource;

        lock (_swapSuggestionsCancellationSourceLock)
        {
            if (currentEpoch != Volatile.Read(ref _lastUpdateSearchTextEpoch))
            {
                currentCancellationSource?.Dispose();

                return;
            }

            previousCancellationSource = Interlocked.Exchange(ref _previousSuggestionsCancellationSource, currentCancellationSource);
        }

        try
        {
            previousCancellationSource?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        var historyItems = BuildHistoryItems(newSearch);

        if (shouldOpenHomePage)
        {
            UpdateSuggestionItems([], currentEpoch);

            RenderItems([_openHomepageListItem, .. historyItems], currentEpoch);

            return;
        }

        var primaryItems = BuildPrimaryItems(newSearch);
        var snapshotSuggestions = Volatile.Read(ref _suggestionItems);

        RenderItems([.. primaryItems, .. historyItems, .. snapshotSuggestions], currentEpoch);

        if (!shouldFetchSuggestions)
            return;

        IListItem[] suggestionItems;
        try
        {
            suggestionItems = await FetchSuggestionItemsAsync(newSearch, currentCancellationSource!.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage("Suggestions fetch failed: " + ex.ToString());

            return;
        }
        finally
        {
            Interlocked.CompareExchange(ref _previousSuggestionsCancellationSource, null, currentCancellationSource);

            currentCancellationSource!.Dispose();
        }

        if (currentEpoch != Volatile.Read(ref _lastUpdateSearchTextEpoch))
            return;

        UpdateSuggestionItems(suggestionItems, currentEpoch);

        RenderItems([.. primaryItems, .. historyItems, .. suggestionItems], currentEpoch);
    }

    private void RenderItems(IListItem[] items, int currentUpdateSearchTextEpoch)
    {
        if (currentUpdateSearchTextEpoch != Volatile.Read(ref _lastUpdateSearchTextEpoch))
            return;

        lock (_renderLock)
        {
            if (currentUpdateSearchTextEpoch != Volatile.Read(ref _lastUpdateSearchTextEpoch))
                return;

            Volatile.Write(ref _items, items);
        }

        RaiseItemsChanged(items.Length);
    }

    private void UpdateSuggestionItems(IListItem[] suggestionItems, int currentUpdateSearchTextEpoch)
    {
        if (currentUpdateSearchTextEpoch != Volatile.Read(ref _lastUpdateSearchTextEpoch))
            return;

        lock (_updateSuggestionLock)
        {
            if (currentUpdateSearchTextEpoch != Volatile.Read(ref _lastUpdateSearchTextEpoch))
                return;

            Volatile.Write(ref _suggestionItems, suggestionItems);
        }
    }

    private ListItem[] BuildPrimaryItems(string searchText)
    {
        return
        [
            new ListItem(
                new SearchWebCommand(_shortcut, searchText)
                {
                    Icon = Icons.Search,
                    Name = StringFormatter.Format(Resources.SearchQueryItem_NameTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = searchText })
                }
            )
            {
                Title = StringFormatter.Format(Resources.SearchQueryItem_TitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = searchText }),
                Subtitle = StringFormatter.Format(Resources.SearchQueryItem_SubtitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = searchText }),
                Icon = Icons.Search,
                MoreCommands = [_openHomepageContextItem]
            }
        ];
    }

    private ListItem[] BuildHistoryItems(string searchText)
    {
        var historyQueries = HistoryService
            .Search(_shortcut.Name, searchText)
            .Take(string.IsNullOrEmpty(searchText) ? MaxDisplayCount : MaxHistoryDisplayCount);

        return [
            .. historyQueries.Select(historyQuery => new ListItem(
                new SearchWebCommand(_shortcut, historyQuery)
                {
                    Icon = Icons.Search,
                    Name = StringFormatter.Format(Resources.SearchQueryItem_NameTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = historyQuery })
                }
            )
            {
                Title = StringFormatter.Format(Resources.SearchQueryItem_TitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = historyQuery }),
                Subtitle = StringFormatter.Format(Resources.SearchQueryItem_SubtitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = historyQuery }),
                Icon = Icons.History,
                TextToSuggest = historyQuery,
                MoreCommands = [
                    _openHomepageContextItem,
                    new CommandContextItem(
                        title: StringFormatter.Format(Resources.DeleteHistory_TitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = historyQuery }),
                        name: $"[UNREACHABLE] DeleteHistory.Name - shortcut='{_shortcut.Name}', query='{historyQuery}'",
                        action: () =>
                        {
                            HistoryService.Remove(_shortcut.Name, historyQuery);
                            Rebuild();
                        },
                        result: CommandResult.KeepOpen()
                    )
                    {
                        Icon = Icons.DeleteHistory,
                        IsCritical = true
                    }
                ]
            })
        ];
    }

    private async Task<ListItem[]> FetchSuggestionItemsAsync(string searchText, CancellationToken cancellationToken)
    {
        var suggestions = await SuggestionsRegistry
            .Get(_shortcut.SuggestionProvider!)
            .GetSuggestionsAsync(searchText, cancellationToken)
            .ConfigureAwait(false);

        return
        [
            .. suggestions.Select(suggestion => new ListItem(
                new SearchWebCommand(_shortcut, suggestion.Title)
                {
                    Icon = Icons.Search,
                    Name = StringFormatter.Format(Resources.SearchQueryItem_NameTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] =  suggestion.Title })
                }
            )
            {
                Title = StringFormatter.Format(Resources.SearchQueryItem_TitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] =  suggestion.Title }),
                Subtitle = suggestion.Description ?? StringFormatter.Format(Resources.SearchQueryItem_SubtitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = suggestion.Title }),
                Icon = Icons.Search,
                TextToSuggest = suggestion.Title,
                MoreCommands = [_openHomepageContextItem]
            })
        ];
    }
}
