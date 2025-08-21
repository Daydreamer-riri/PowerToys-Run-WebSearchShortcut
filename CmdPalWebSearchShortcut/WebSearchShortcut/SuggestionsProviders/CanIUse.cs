using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.SuggestionsProviders;

internal sealed class CanIUse : ISuggestionsProvider
{
    public string Name => "CanIUse";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query)
    {
        try
        {
            const string api = "https://caniuse.com/process/query.php?search=";

            await using var resultStream = await Http
                .GetStreamAsync(api + Uri.EscapeDataString(query))
                .ConfigureAwait(false);

            using var json = await JsonDocument
                .ParseAsync(resultStream)
                .ConfigureAwait(false);

            var featureIds = json
                .RootElement.GetProperty("featureIds")
                .EnumerateArray()
                .Take(10);

            Suggestion[] items = [
                .. featureIds
                    .Select(o =>
                    {
                        var title = o.GetString();
                        return title is null
                            ? null
                            : new Suggestion(title);
                    })
                    .Where(s => s != null)
                    .Select(s => s!)
            ];

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
