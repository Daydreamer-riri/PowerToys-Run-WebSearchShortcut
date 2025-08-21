using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.SuggestionsProviders;

internal sealed class Wikipedia : ISuggestionsProvider
{
    public string Name => "Wikipedia";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query)
    {
        try
        {
            const string api = "https://api.wikimedia.org/core/v1/wikipedia/en/search/title?q=";

            await using var resultStream = await Http
                .GetStreamAsync(api + Uri.EscapeDataString(query))
                .ConfigureAwait(false);

            using var json = await JsonDocument
                .ParseAsync(resultStream)
                .ConfigureAwait(false);

            var results = json.RootElement.GetProperty("pages");

            Suggestion[] items = [
                .. results
                    .EnumerateArray()
                    .Select(o => (
                        Title: o.TryGetProperty("title", out var t) ? t.GetString() : null,
                        Description: o.TryGetProperty("description", out var d) ? d.GetString() : null
                    ))
                    .Where(p => !string.IsNullOrWhiteSpace(p.Title))
                    .Select(p => new Suggestion(p.Title!, p.Description ?? ""))
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
        return "Wikipedia";
    }
}
