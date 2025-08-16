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
    private CancellationTokenSource? _suggestionsCancellationTokenSource;

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

    public override IListItem[] GetItems() => _items;

    public override async void UpdateSearchText(string oldSearch, string newSearch)
    {
        var currentCancellation = new CancellationTokenSource();
        var previousCancellation = Interlocked.Exchange(ref _suggestionsCancellationTokenSource, currentCancellation);
        previousCancellation?.Cancel();
        previousCancellation?.Dispose();

        if (string.IsNullOrEmpty(newSearch))
        {
            _suggestionItems = [];

            RenderItems([_openHomePageItem]);

            return;
        }

        var primaryItems = BuildPrimaryItems(newSearch);

        RenderItems([.. primaryItems, .. _suggestionItems]);

        if (string.IsNullOrEmpty(_shortcut.SuggestionProvider))
            return;

        ListItem[] suggestionItems;
        try
        {
            suggestionItems = await GetSuggestionItemsAsync(newSearch, currentCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (!ReferenceEquals(_suggestionsCancellationTokenSource, currentCancellation) || currentCancellation.IsCancellationRequested)
            return;

        _suggestionItems = suggestionItems;

        RenderItems([.. primaryItems, .. _suggestionItems]);
    }

    private void RenderItems(IListItem[] items)
    {
        _items = items;

        RaiseItemsChanged(_items.Length);
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

    private async Task<ListItem[]> GetSuggestionItemsAsync(string searchText, CancellationToken cancellationToken)
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
