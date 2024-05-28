using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.WebSearchShortcut.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut
{
  /// <summary>
  /// File storage.
  /// </summary>
  public interface IWebSearchShortcutStorage
  {
    /// <summary>
    /// Gets all records.
    /// </summary>
    /// <returns>The records.</returns>
    IReadOnlyCollection<Item> GetRecords();

    public Item? DefaultItem { get; }

    /// <summary>
    /// Gets matching records.
    /// </summary>
    /// <param name="query">A key/value query.</param>
    /// <returns>The records.</returns>
    IReadOnlyCollection<Item> GetRecords(string query);

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="includeNoPlaceholder"></param>
    /// <returns>The record.</returns>
    Item? GetRecord(string key, bool includeNoPlaceholder = false);

    /// <summary>
    /// Loads file.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves file.
    /// </summary>
    void Save();
    string GetPath();

    string? LoadError { get; }
  }

  /// <inheritdoc/>
  public class WebSearchShortcutStorage : IWebSearchShortcutStorage
  {
    /// <summary>
    /// Default file name.
    /// </summary>
    public const string DefaultFileName = "WebSearchShortcutStorage.json";

    public string? LoadError { get; private set; }

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
      IncludeFields = true,
      PropertyNameCaseInsensitive = true,
      WriteIndented = true,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSearchShortcutStorage"/> class.
    /// </summary>
    /// <param name="settings">Plugin settings.</param>
    public WebSearchShortcutStorage(WebSearchShortcutSettings settings)
    {
      Settings = settings;

      Load();
    }

    private WebSearchShortcutSettings Settings { get; }

    private Dictionary<string, Item> Data { get; set; } = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<Item> GetRecords() => Data.Values.ToList().AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyCollection<Item> GetRecords(string query) => Data.Values
    // .Where(x => x.IsDefault != true)
    .Where(x =>
        (x.Keyword?.Contains(query, StringComparison.InvariantCultureIgnoreCase) ?? false)
        || (x.Name?.Contains(query, StringComparison.InvariantCultureIgnoreCase) ?? false)
      ).ToList().AsReadOnly();

    /// <inheritdoc/>
    public Item? GetRecord(string key, bool includeNoPlaceholder = false)
    {
      key = key.Trim();
      return Data.Values
      // .Where(x => x.IsDefault != true)
      .FirstOrDefault(x =>
        ((x.Name?.Equals(key, StringComparison.InvariantCultureIgnoreCase) ?? false)
        || (x.Keyword?.Equals(key, StringComparison.InvariantCultureIgnoreCase) ?? false)
        ) && (includeNoPlaceholder || x.Url.Contains("%s")));
    }

    public Item? DefaultItem => Data.Values.FirstOrDefault(x => x?.IsDefault == true, null);

    /// <inheritdoc/>
    public bool RemoveRecord(string key) => Data.Remove(key);

    /// <inheritdoc/>
    public void Load()
    {
      var path = GetPath();

      if (!File.Exists(path))
      {
        var initData = new Dictionary<string, InitConfigItem>
        {
          { "Google", new InitConfigItem { Url = "https://www.google.com/search?q=%s", SuggestionProvider = "Google" } },
          { "Bing", new InitConfigItem { Url = "https://www.bing.com/search?q=%s", SuggestionProvider = "Bing" } },
          { "GitHub", new InitConfigItem { Url = "https://www.github.com/search?q=%s", Keyword = "gh" } },
          { "YouTube", new InitConfigItem{ Url = "https://www.youtube.com/results?search_query=%s", Keyword : "yt" } },
          { "ChatGPT", new  InitConfigItem{ Url = "https://chat.openai.com/?q=%s", Keyword : "gpt" } }

        };

        var json = JsonSerializer.Serialize(initData, _serializerOptions);
        File.WriteAllText(path, json);
      }

      try
      {
        var json = File.ReadAllText(path);
        Data = JsonSerializer.Deserialize<Dictionary<string, Item>>(json, _serializerOptions) ?? [];
        foreach (var (key, item) in Data)
        {
          item.Name = key;
          if (item.Urls != null && item.Urls.Length > 0)
          {
            item.Url = string.Join(" ", item.Urls);
          }
        }
        DownLoadIcon();

        LoadError = null;
      }
      catch (Exception ex)
      {
        LoadError = ex.Message;
        Log.Exception("Load failed: " + path, ex, GetType());
      }
    }

    private async void DownLoadIcon()
    {
      string iconDirectory = Path.Combine(Main.PluginDirectory, "Images", "Icons");
      if (Directory.Exists(iconDirectory))
      {
        Directory.Delete(iconDirectory, true);
      }
      List<Task<bool>> tasks = Data.Values.Select(x => x.DownLoadIcon()).ToList();
      await Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public void Save()
    {
      var path = GetPath();

      try
      {
        var json = JsonSerializer.Serialize(Data, _serializerOptions);
        File.WriteAllText(path, json);
      }
      catch (Exception ex)
      {
        Log.Exception("Save failed: " + path, ex, GetType());
      }
    }

    public string GetPath() => Path.Combine(Settings.StorageDirectoryPath, Settings.StorageFileName);
  }

  public class InitConfigItem
  {
    public required string Url { get; set; }
    public string? SuggestionProvider { get; set; }
    public string? Keyword { get; set; }
  }
}