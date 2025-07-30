using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.SuggestionsProvider;
public class CanIUse : IWebSearchShortcutSuggestionsProvider
{
  public static string Name => "CanIUse";

  private HttpClient Http { get; } = new HttpClient();

  public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
  {
    try
    {
      const string api = "https://caniuse.com/process/query.php?search=";

      await using var resultStream = await Http.GetStreamAsync( api + Uri.EscapeDataString(query))
          .ConfigureAwait(false);

      using var json = await JsonDocument.ParseAsync(resultStream);
      var featureIds = json
          .RootElement.GetProperty("featureIds")
          .EnumerateArray()
          .Take(10);

      List<SuggestionsItem> items = featureIds
          .Select(o =>
          {
            var title = o.GetString();
            return title is null
              ? null
              : new SuggestionsItem(title);
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
    return "CanIUse";
  }
}
