using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.SuggestionsProvider;
public class Bing : IWebSearchShortcutSuggestionsProvider
{
  public static string Name => "Bing";

  private HttpClient Http { get; } = new HttpClient();

  public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
  {
    try
    {
      const string api = "https://api.bing.com/qsonhs.aspx?q=";

      await using var resultStream = await Http.GetStreamAsync(
              api + Uri.EscapeDataString(query)
          )
          .ConfigureAwait(false);

      using var json = await JsonDocument.ParseAsync(resultStream);
      var root = json.RootElement.GetProperty("AS");

      if (root.GetProperty("FullResults").GetInt32() == 0)
        return [];

      List<string> titles = root.GetProperty("Results")
          .EnumerateArray()
          .SelectMany(r =>
              r.GetProperty("Suggests")
                  .EnumerateArray()
                  .Select(s => s.GetProperty("Txt").GetString())
          )
          .Where(s => s != null)
          .Select(s => s!)
          .ToList();

      return titles
        .Select(t => new SuggestionsItem(
          t,
          // Resources.search_for.Replace("%search", $"\"{t}\"")
          Resources.SuggestionsProvider_Description
        ))
        .ToList();
    }
    catch (Exception e)
        when (e is HttpRequestException or { InnerException: TimeoutException })
    {
      ExtensionHost.LogMessage($"{e.Message}");
      return [];
    }
  }

  public override string ToString()
  {
    return "Bing";
  }
}