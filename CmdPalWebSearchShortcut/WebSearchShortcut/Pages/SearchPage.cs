using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Commands;

namespace WebSearchShortcut;

public partial class SearchPage : DynamicListPage
{
  public string Url { get; }
  public WebSearchShortcutItem Item { get; }

  private List<ListItem> allItems;

  public SearchPage(WebSearchShortcutItem data)
  {
    Item = data;
    Name = data.Name;
    Url = data.Url;
    Icon = !string.IsNullOrWhiteSpace(data.IconUrl) ? new IconInfo(data.IconUrl) : new IconInfo(IconFromUrl(Url));
    allItems = [new(new NoOpCommand())
        {
            // Icon = IconHelpers.FromRelativePath("Assets\\WebSearch.png"),
            Title = $"Search using {data.Name}",
            // Subtitle = string.Format(CultureInfo.CurrentCulture, PluginOpen, BrowserInfo.Name ?? BrowserInfo.MSEdgeName),
        }
        ];
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
      results.Add(new ListItem(new NoOpCommand())
      {
        Title = $"Search {Name}",
        // Subtitle = string.Format(CultureInfo.CurrentCulture, PluginInBrowserName, BrowserInfo.Name ?? BrowserInfo.MSEdgeName),
        // Icon = new IconInfo(_iconPath),
      });
    }
    else
    {
      var searchTerm = query;
      var result = new ListItem(new SearchWebCommand(searchTerm))
      {
        Title = searchTerm,
        Subtitle = $"Search {Name} for '{searchTerm}'",
        // Icon = new IconInfo(_iconPath),
      };
      results.Add(result);
    }

    return results;
  }

  public override void UpdateSearchText(string oldSearch, string newSearch)
  {
    allItems = [.. Query(newSearch)];
    RaiseItemsChanged();
  }
  // public override CommandResult Invoke()
  // {
  //   var target = Url;
  //   try
  //   {
  //     var uri = GetUri(target);
  //     if (uri != null)
  //     {
  //       _ = Launcher.LaunchUriAsync(uri);
  //     }
  //     else
  //     {
  //       // throw new UriFormatException("The provided URL is not valid.");
  //     }
  //   }
  //   catch (Exception ex)
  //   {
  //     System.Diagnostics.Debug.WriteLine($"Error launching URL: {ex.Message}");
  //   }

  //   return CommandResult.Dismiss();
  // }

  internal static Uri? GetUri(string url)
  {
    Uri? uri;
    if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
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