using System.Collections.Generic;
using System.Linq;
using Community.PowerToys.Run.Plugin.WebSearchShortcut.Models;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut.Helpers
{
  public interface IQuery
  {
    IEnumerable<Item> GetAll();

    IEnumerable<Item> Search(string query);
  }

  public class Query: IQuery {
    private readonly IWebSearchShortcutStorage _webSearchShortcutStorage;
    public Query(IWebSearchShortcutStorage webSearchShortcutStorage)
    {
      _webSearchShortcutStorage = webSearchShortcutStorage;
    }
    public IEnumerable<Item> Search(string query)
    {
      var path = query.Replace('\\', '/').Split('/');
      return _webSearchShortcutStorage.GetRecords(query);

      // if (_profileManager.FavoriteProviders.Count == 1)
      // {
      //   return Search(_profileManager.FavoriteProviders[0].Root, path, 0);
      // }
      // else
      // {
      //   var results = new List<Item>();

      //   foreach (var root in _webSearchShortcutStorage.GetRecords())
      //   {
      //     results.AddRange(Search(root, path, 0));
      //   }

      //   // Flatten folders with same path for each profiles
      //   return results.DistinctBy(f => new { f.Path, f.Type, f.Profile });
      // }
    }

    public IEnumerable<Item> GetAll()
    {
      return _webSearchShortcutStorage.GetRecords();
    }

    // private IEnumerable<Item> Search(Item node, string query)
    // {
    //   return [.. _webSearchShortcutStorage.GetRecords(query)];
    // }
  }
}