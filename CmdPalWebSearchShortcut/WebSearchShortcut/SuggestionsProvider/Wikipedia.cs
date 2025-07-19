using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.SuggestionsProvider;

class Wikipedia : IWebSearchShortcutSuggestionsProvider
{
  public static string Name => "Wikipedia";

  private HttpClient Http { get; } = new HttpClient();

  public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
  {
    try
    {
      const string api = "https://api.wikimedia.org/core/v1/wikipedia/en/search/title?q=";

      await using var resultStream = await Http.GetStreamAsync(
              api + Uri.EscapeDataString(query)
          )
          .ConfigureAwait(false);

      using var json = await JsonDocument.ParseAsync(resultStream);

      var results = json.RootElement.GetProperty("pages");

      List<string> titles = results
          .EnumerateArray()
          .Select(o => o.GetProperty("title").GetString())
          .Where(s => !string.IsNullOrEmpty(s) && !s.Equals(query, StringComparison.OrdinalIgnoreCase))
          .Select(s => s!)
          .ToList();

      return titles
          .Select(t => new SuggestionsItem(
              t,
              $"Search for \"{t}\""
          ))
          .ToList();
    }
    catch (Exception e)
    {
      ExtensionHost.LogMessage($"{e.Message}");
      return [];
    }
  }

  public override string ToString()
  {
    return "Wikipedia";
  }
}
