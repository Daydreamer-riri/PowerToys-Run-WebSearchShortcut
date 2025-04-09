using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.SuggestionsProvider;

class Google : IWebSearchShortcutSuggestionsProvider
{
  public static string Name => "Google";

  private HttpClient Http { get; } = new HttpClient();

  public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
  {
    try
    {
      const string api = "https://www.google.com/complete/search?output=chrome&q=";

      await using var resultStream = await Http.GetStreamAsync(
              api + Uri.EscapeDataString(query)
          )
          .ConfigureAwait(false);

      using var json = await JsonDocument.ParseAsync(resultStream);

      var results = json.RootElement.EnumerateArray().ElementAt(1);

      List<string> titles = results
          .EnumerateArray()
          .Select(o => o.GetString())
          .Where(s => s != null)
          .Select(s => s!)
          .ToList();

      return titles
          .Select(t => new SuggestionsItem(
              t,
              // Resources.search_for.Replace("%search", $"\"{t}\"")
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
    return "Google";
  }
}
