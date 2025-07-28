using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.SuggestionsProvider;

class DuckDuckGo : IWebSearchShortcutSuggestionsProvider
{
  public static string Name => "DuckDuckGo";

  private HttpClient Http { get; } = new HttpClient();

  public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
  {
    try
    {
      const string api = "https://duckduckgo.com/ac/?q=";

      await using var resultStream = await Http.GetStreamAsync(
              api + Uri.EscapeDataString(query)
          )
          .ConfigureAwait(false);

      using var json = await JsonDocument.ParseAsync(resultStream);

      var results = json.RootElement;

      List<string> titles = results
          .EnumerateArray()
          .Select(o => o.GetProperty("phrase").GetString())
          .Where(s => !string.IsNullOrEmpty(s) && !s.Equals(query, StringComparison.OrdinalIgnoreCase))
          .ToList()!;

      return titles
          .Select(t => new SuggestionsItem(
              t,
              Resources.SuggestionsProvider_Description
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
    return "DuckDuckGo";
  }
}
