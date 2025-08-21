using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSearchShortcut.SuggestionsProviders;

namespace WebSearchShortcut;

internal interface ISuggestionsProvider
{
    string Name { get; }
    Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default);
}

internal sealed record Suggestion(string Title, string? Description = null);

internal static class SuggestionsRegistry
{
    public static IReadOnlyCollection<string> ProviderNames => _suggestionsProviders.Keys;

    public static ISuggestionsProvider Get(string name) =>
        _suggestionsProviders.TryGetValue(name, out var provider)
            ? provider
            : throw new KeyNotFoundException($"Unknown suggestion provider: '{name}'.");

    public static bool TryGet(string name, out ISuggestionsProvider provider) =>
        _suggestionsProviders.TryGetValue(name, out provider!);

    private static readonly Dictionary<string, ISuggestionsProvider> _suggestionsProviders =
        new ISuggestionsProvider[]
        {
            new Google(),
            new Bing(),
            new DuckDuckGo(),
            new YouTube(),
            new Wikipedia(),
            new Npm(),
            new CanIUse(),
        }.ToDictionary(provider => provider.Name, StringComparer.OrdinalIgnoreCase);
}
