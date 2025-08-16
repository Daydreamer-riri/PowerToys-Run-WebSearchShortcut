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
    private int _updateEpoch;

    public SearchWebPage(WebSearchShortcutDataEntry shortcut)
    {
        _shortcut = shortcut;

        Name = shortcut.Name;
        Icon = IconService.GetIconInfo(shortcut);
        _openHomePageItem = new ListItem(new OpenHomePageCommand(shortcut))
        {
            Title = StringFormatter.Format(Resources.OpenHomePage_TitleTemplate, new() { ["engine"] = Name })
        };

        _updateEpoch = 0;

        _items = [_openHomePageItem];
    }

    public override IListItem[] GetItems() => _items;

    public override async void UpdateSearchText(string oldSearch, string newSearch)
    {
        var capturedEpoch = Interlocked.Increment(ref _updateEpoch);

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

        var suggestionItems = await GetSuggestionItemsAsync(newSearch);

        if (capturedEpoch != _updateEpoch)
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

    private async Task<ListItem[]> GetSuggestionItemsAsync(string searchText)
    {
        var suggestions = await SuggestionsRegistry
            .Get(_shortcut.SuggestionProvider!)
            .GetSuggestionsAsync(searchText)
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
