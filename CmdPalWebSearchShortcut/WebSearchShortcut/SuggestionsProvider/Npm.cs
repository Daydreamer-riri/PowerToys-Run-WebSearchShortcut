using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.SuggestionsProvider;

public class Npm : IWebSearchShortcutSuggestionsProvider
{
  public static string Name => "Npm";

  private HttpClient Http { get; } = new HttpClient();

  public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
  {
    try
    {
      const string api = "https://www.npmjs.com/search/suggestions?q=";

      await using var resultStream = await Http.GetStreamAsync(
              api + Uri.EscapeDataString(query)
          )
          .ConfigureAwait(false);

      using var json = await JsonDocument.ParseAsync(resultStream);

      var results = json.RootElement.EnumerateArray();

      List<SuggestionsItem> items = results
          .Select(o =>
          {
            var title = o.GetProperty("name").GetString();
            var description = o.GetProperty("description").GetString();
            return title is null ? null : new SuggestionsItem(title, description ?? "");
          })
          .Where(s => s is not null)
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
    return "Npm";
  }
}
