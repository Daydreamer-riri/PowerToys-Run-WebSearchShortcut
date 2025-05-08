
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.SuggestionsProvider;
public class YouTube : IWebSearchShortcutSuggestionsProvider
{
  public static string Name => "YouTube";

  private HttpClient Http { get; } = new HttpClient();

  public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
  {
    try
    {
      const string api = "https://suggestqueries-clients6.youtube.com/complete/search?ds=yt&client=youtube&gs_ri=youtube&q=";

      var result = await Http.GetStringAsync(api + Uri.EscapeDataString(query));

      var match = Regex.Match(result, @"window\.google\.ac\.h\((.*)\)$");
      if (!match.Success)
      {
        ExtensionHost.LogMessage("No match found in the response.");
        return new List<SuggestionsItem>();
      }

      var jsonContent = match.Groups[1].Value;
      using var json = JsonDocument.Parse(jsonContent);
      var results = json.RootElement[1].EnumerateArray();

      List<SuggestionsItem> items = results
          .Select(o =>
          {
            var title = o[0].GetString();
            var description = $"Search for \"{title}\"";
            return title == null ? null : new SuggestionsItem(title, description ?? "");
          })
          .Where(s => s != null)
          .Select(s => s!)
          .ToList();

      return items;
    }
    catch (Exception e)
    {
      ExtensionHost.LogMessage($"{e.Message}");
      return [];
    }
  }

  public override string ToString()
  {
    return "YouTube";
  }
}