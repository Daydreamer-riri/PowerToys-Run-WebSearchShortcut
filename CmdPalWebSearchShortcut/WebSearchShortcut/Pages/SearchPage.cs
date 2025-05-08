using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Commands;
using Windows.System;

namespace WebSearchShortcut;

public partial class SearchPage : DynamicListPage
{
  public string Url { get; }
  public WebSearchShortcutItem Item { get; }

  private List<ListItem> allItems;
  private readonly IconInfo _searchIcon = new("\uE721");
  private List<ListItem> allSuggestItems;
  private readonly ListItem _emptyListItem;

  public SearchPage(WebSearchShortcutItem data)
  {
    Item = data;
    Name = data.Name;
    Url = data.Url;
    Icon = !string.IsNullOrWhiteSpace(data.IconUrl) ? new IconInfo(data.IconUrl) : new IconInfo(IconFromUrl(Url));
    _emptyListItem = new ListItem(new OpenDomainCommand(data));
    allItems = [_emptyListItem];

    _lastSuggestionId = 0;
    allSuggestItems = [];
  }

  public override IListItem[] GetItems()
  {
    return [
      ..allItems,
    ];
  }

  public List<ListItem> Query(string query)
  {
    var results = new List<ListItem>();
    // empty query
    if (string.IsNullOrEmpty(query))
    {
      allSuggestItems = [];
      results.Add(_emptyListItem);
    }
    else
    {
      var searchTerm = query;
      var result = new ListItem(new SearchWebCommand(searchTerm, Item))
      {
        Title = searchTerm,
        Subtitle = $"Search {Name} for '{searchTerm}'",
        MoreCommands = [new CommandContextItem(
          title: $"Open {Name}",
          name: $"Open {Name}",
          action: () =>
          {
            var uri = GetUri(Item.Domain);
            if (uri != null)
            {
              _ = Launcher.LaunchUriAsync(uri);
            }
          }
        )]
      };
      results.Add(result);
    }

    return results;
  }

  private int _lastSuggestionId;
  public override async void UpdateSearchText(string oldSearch, string newSearch)
  {
    var ignoreId = ++_lastSuggestionId;
    var queryItems = Query(newSearch);
    allItems = [.. queryItems, .. allSuggestItems];
    RaiseItemsChanged(allItems.Count);
    if (string.IsNullOrWhiteSpace(Item.SuggestionProvider) || string.IsNullOrEmpty(newSearch))
    {
      return;
    }
    var suggestions = await Suggestions.QuerySuggestionsAsync(Item.SuggestionProvider, newSearch);
    if (ignoreId != _lastSuggestionId)
    {
      return;
    }
    List<ListItem> suggestItems = [.. suggestions
      .Select(s => new ListItem(new SearchWebCommand(s.Title, Item))
      {
        Title = s.Title,
        Subtitle = s.Description ?? "",
        // TextToSuggest = s.Title,
        MoreCommands = [new CommandContextItem(
          title: $"Open {Name}",
          name: $"Open {Name}",
          action: () =>
          {
            var uri = GetUri(Item.Domain);
            if (uri != null)
            {
              _ = Launcher.LaunchUriAsync(uri);
            }
          }
        )]
      })];
    List<ListItem> items = [.. queryItems, .. suggestItems];
    allSuggestItems = suggestItems;

    allItems = items;
    RaiseItemsChanged(allItems.Count);
  }
  internal static Uri? GetUri(string url)
  {
    if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
    {
      if (!Uri.TryCreate("https://" + url, UriKind.Absolute, out uri))
      {
        return null;
      }
    }

    return uri;
  }

  private static string IconFromUrl(string url)
  {
    var baseString = url.Split(' ')[0].Split('?')[0];
    try
    {
      var uri = GetUri(baseString);
      if (uri != null)
      {
        var hostname = uri.Host;
        var faviconUrl = $"{uri.Scheme}://{hostname}/favicon.ico";
        return faviconUrl;
      }
    }
    catch (UriFormatException)
    {
      // return "🔗";
    }

    return "🔗";
  }

  public static async Task<string> IconFromUrlFallback(Uri uri)
  {

    using HttpClient client = new();
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (compatible; AcmeInc/1.0)"
    );

    var hostname = uri.Host;
    var faviconUrl = $"{uri.Scheme}://{hostname}/favicon.ico";
    try
    {
      HttpResponseMessage response = await client.GetAsync(faviconUrl);
      if (
          response.IsSuccessStatusCode
          && response.Content.Headers.ContentType?.MediaType != "text/html"
      )
      {
        return faviconUrl;
      }
    }
    catch (System.Exception)
    {
    }
    var iconUrl = $"https://www.google.com/s2/favicons?sz=64&domain={uri.GetLeftPart(UriPartial.Authority)}";
    return iconUrl;
  }
}