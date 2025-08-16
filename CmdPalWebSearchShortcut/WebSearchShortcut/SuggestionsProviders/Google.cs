using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.SuggestionsProviders;

internal sealed class Google : ISuggestionsProvider
{
    public string Name => "Google";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query)
    {
        try
        {
            const string api = "https://www.google.com/complete/search?output=chrome&q=";

            await using var resultStream = await Http
                .GetStreamAsync(api + Uri.EscapeDataString(query))
                .ConfigureAwait(false);

            using var json = await JsonDocument
                .ParseAsync(resultStream)
                .ConfigureAwait(false);

            var results = json.RootElement.EnumerateArray().ElementAt(1);

            string[] titles = [
                .. results
                    .EnumerateArray()
                    .Select(o => o.GetString())
                    .Where(s => s is not null)
                    .Select(s => s!)
            ];

            return [.. titles.Select(t => new Suggestion(t))];
        }
        catch (Exception e)
        {
            ExtensionHost.LogMessage($"{e.Message}");

            return [];
        }
    }

    public override string ToString()
    {
        return "Google";
    }
}
