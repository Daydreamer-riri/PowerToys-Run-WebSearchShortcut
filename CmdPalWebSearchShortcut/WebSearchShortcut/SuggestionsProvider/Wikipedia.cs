using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

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

      var titleDescriptionPairs = results
          .EnumerateArray()
          .Where(o =>
              o.TryGetProperty("title", out var titleProp) &&
              !string.IsNullOrWhiteSpace(titleProp.GetString())
          )
          .Select(o =>
          {
              string title = o.GetProperty("title").GetString()!;
              string? description = o.TryGetProperty("description", out var descProp)
                  ? descProp.GetString()
                  : null;

              return (title, description);
          });

          return [
              .. titleDescriptionPairs.Select(pair =>
                  new SuggestionsItem(
                      pair.title,
                      pair.description ?? Resources.SuggestionsProvider_Description
                  )
              )
          ];
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
