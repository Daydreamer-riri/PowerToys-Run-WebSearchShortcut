using System.Collections.Generic;
using System.Threading.Tasks;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut.Suggestion
{
  public interface IWebSearchShortcutSuggestionsProvider
  {
    Task<List<SuggestionsItem>> QuerySuggestionsAsync(string query);
  }

  public class SuggestionsItem(string title, string description)
    {
        public string Title { get; } = title;
        public string? Description { get; } = description;
    }

  public class Suggestions
  {
    public Task<List<SuggestionsItem>> QuerySuggestionsAsync(string name, string query)
    {
      var provider = SuggestionProviders[name];
      if (provider != null)
      {
        return provider.QuerySuggestionsAsync(query);
      }
      return Task.FromResult(new List<SuggestionsItem>());
    }
    public Dictionary<string, IWebSearchShortcutSuggestionsProvider> SuggestionProviders { get; set; } = new(){
      { Google.Name, new Google() },
      { Bing.Name, new Bing() },
      { Npm.Name, new Npm() },
      { CanIUse.Name, new CanIUse() }
    };
  }
}