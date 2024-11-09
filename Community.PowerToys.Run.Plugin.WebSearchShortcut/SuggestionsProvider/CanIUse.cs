using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.WebSearchShortcut.Properties;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut.Suggestion
{
    public class CanIUse : IWebSearchShortcutSuggestionsProvider
    {
        public static string Name => "CanIUse";

        private HttpClient Http { get; } = new HttpClient();

        public async Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query)
        {
            try
            {
                const string api = "https://caniuse.com/process/query.php?search=";

                await using var resultStream = await Http.GetStreamAsync(
                        api + Uri.EscapeDataString(query)
                    )
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
                        return title == null
                            ? null
                            : new SuggestionsItem(
                                title,
                                Resources.search_for.Replace("%search", $"\"{title}\"")
                            );
                    })
                    .Where(s => s != null)
                    .Select(s => s!)
                    .ToList();

                return items;
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}", typeof(CanIUse));
                return [];
            }
        }

        public override string ToString()
        {
            return "CanIUse";
        }
    }
}
