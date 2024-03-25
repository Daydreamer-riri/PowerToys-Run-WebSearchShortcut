using System.Collections.Generic;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut.Suggestions
{
  interface IWebSearchShortcutSuggestions
  {
      IReadOnlyCollection<SuggestionsItem> GetSuggestions();
  }

  interface IWebSearchShortcutSuggestionsProvider
  {
    string Name { get; }
    IReadOnlyCollection<SuggestionsItem> GetSuggestions(string query);
  }

  public class SuggestionsItem(string title, string? description)
  {
    public string Title { get; } = title;
    public string? Description { get; } = description;
  }
}