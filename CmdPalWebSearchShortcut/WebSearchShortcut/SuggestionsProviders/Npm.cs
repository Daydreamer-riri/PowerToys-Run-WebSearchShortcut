using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.SuggestionsProviders;

internal sealed class Npm : ISuggestionsProvider
{
    public string Name => "Npm";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query)
    {
        try
        {
            const string api = "https://www.npmjs.com/search/suggestions?q=";

            await using var resultStream = await Http
                .GetStreamAsync(api + Uri.EscapeDataString(query))
                .ConfigureAwait(false);

            using var json = await JsonDocument
                .ParseAsync(resultStream)
                .ConfigureAwait(false);

            var results = json.RootElement.EnumerateArray();

            Suggestion[] items = [
                .. results
                    .Select(o =>
                    {
                        var title = o.GetProperty("name").GetString();
                        var description = o.GetProperty("description").GetString();
                        return title is null ? null : new Suggestion(title, description ?? "");
                    })
                    .Where(s => s is not null)
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
        return "Npm";
    }
}
