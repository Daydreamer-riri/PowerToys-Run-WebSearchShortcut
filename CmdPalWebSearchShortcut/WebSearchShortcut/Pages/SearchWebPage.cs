using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Commands;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Services;

namespace WebSearchShortcut;

internal sealed partial class SearchWebPage : DynamicListPage
{
    private readonly WebSearchShortcutDataEntry _shortcut;
    private readonly IListItem _openHomePageItem;
    private IListItem[] _items = [];
    private IListItem[] _suggestionItems = [];
    private int _lastUpdateSearchTextEpoch;
    private readonly Lock _swapSuggestionsCancellationSourceLock = new();
    private readonly Lock _renderLock = new();
    private readonly Lock _updateSuggestionLock = new();
    private CancellationTokenSource? _previousSuggestionsCancellationSource;

    public SearchWebPage(WebSearchShortcutDataEntry shortcut)
    {
        _shortcut = shortcut;

        Name = shortcut.Name;
        Icon = IconService.GetIconInfo(shortcut);
        _openHomePageItem = new ListItem(new OpenHomePageCommand(shortcut))
        {
            Title = StringFormatter.Format(Resources.OpenHomePage_TitleTemplate, new() { ["engine"] = Name })
        };

        _items = [_openHomePageItem];
    }

    public override IListItem[] GetItems() => Volatile.Read(ref _items);

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

        if (shouldOpenHomePage)
        {
            UpdateSuggestionItems([], currentEpoch);

            RenderItems([_openHomePageItem], currentEpoch);

            return;
        }

        var primaryItems = BuildPrimaryItems(newSearch);
        var snapshotSuggestions = Volatile.Read(ref _suggestionItems);

        RenderItems([.. primaryItems, .. snapshotSuggestions], currentEpoch);

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

        if (currentEpoch != Volatile.Read(ref _lastUpdateSearchTextEpoch)) return;

        UpdateSuggestionItems(suggestionItems, currentEpoch);

        RenderItems([.. primaryItems, .. suggestionItems], currentEpoch);
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
            new ListItem(new SearchWebCommand(_shortcut, searchText))
            {
                Title = searchText,
                Subtitle = StringFormatter.Format(Resources.SearchQuery_SubtitleTemplate, new() { ["engine"] = Name, ["query"] = searchText }),
                MoreCommands = [new CommandContextItem(new OpenHomePageCommand(_shortcut))]
            }
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
            .. suggestions.Select(suggestion => new ListItem(new SearchWebCommand(_shortcut, suggestion.Title))
            {
                Title = suggestion.Title,
                Subtitle = suggestion.Description ?? StringFormatter.Format(Resources.SearchQuery_SubtitleTemplate, new() { ["engine"] = Name, ["query"] = suggestion.Title }),
                TextToSuggest = suggestion.Title,
                MoreCommands = [new CommandContextItem(new OpenHomePageCommand(_shortcut))]
            })
        ];
    }
}
