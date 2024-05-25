using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Wox.Plugin.Logger;
using Community.PowerToys.Run.Plugin.WebSearchShortcut.Properties;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut.Suggestion
{
  public class Google : IWebSearchShortcutSuggestionsProvider
  {
    static public string Name => "Google";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
    {
      try
      {
        const string api = "https://www.google.com/complete/search?output=chrome&q=";

        await using var resultStream = await Http.GetStreamAsync(api + Uri.EscapeDataString(query)).ConfigureAwait(false);

        using var json = await JsonDocument.ParseAsync(resultStream);

        var results = json.RootElement.EnumerateArray().ElementAt(1);

        List<string> titles = results
          .EnumerateArray()
          .Select(o => o.GetString())
          .Where(s => s != null)
          .Select(s => s!)
          .ToList();

        return titles.Select(t => new SuggestionsItem(t, Resources.search_for.Replace("%search", t))).ToList();
      }
      catch (Exception e)
      {
        Log.Error($"{e.Message}", typeof(Google));
        return [];
      }
    }

    public override string ToString()
    {
      return "Google";
    }
  }
}
