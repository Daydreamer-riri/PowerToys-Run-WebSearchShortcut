using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    /// <returns>The record.</returns>
    Item? GetRecord(string key);

    // /// <summary>
    // /// Sets the value.
    // /// </summary>
    // /// <param name="key">The key.</param>
    // /// <param name="value">The value.</param>
    // void SetRecord(string key, string value);

    /// <summary>
    /// Removes the record.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The status.</returns>
    bool RemoveRecord(string key);

    /// <summary>
    /// Loads file.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves file.
    /// </summary>
    void Save();

    string Json { get; set; }
  }

  /// <inheritdoc/>
  public class WebSearchShortcutStorage : IWebSearchShortcutStorage
  {
    /// <summary>
    /// Default file name.
    /// </summary>
    public const string DefaultFileName = "WebSearchShortcutStorage.json";

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
    public IReadOnlyCollection<Item> GetRecords(string query) => Data.Values.Where(x =>
        x.KeyWord.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
        x.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase))
        .ToList().AsReadOnly();

    /// <inheritdoc/>
    public Item? GetRecord(string key) => Data.GetValueOrDefault(key);

    // / <inheritdoc/>
    // public void SetRecord(string key, string value)
    // {
    //   if (Data.TryGetValue(key, out Item? record))
    //   {
    //     record.Value = value;
    //     record.Updated = DateTime.UtcNow;
    //   }
    //   else
    //   {
    //     Data[key] = new Item { Key = key, Value = value, Created = DateTime.UtcNow };
    //   }
    // }

    /// <inheritdoc/>
    public bool RemoveRecord(string key) => Data.Remove(key);

    public string Json { get; set; } = "uninit";
    /// <inheritdoc/>
    public void Load()
    {
      var path = GetPath();

      if (!File.Exists(path))
      {
        File.WriteAllText(path, "{}");
      }

      try
      {
        var json = File.ReadAllText(path);
        Data = JsonSerializer.Deserialize<Dictionary<string, Item>>(json, _serializerOptions) ?? [];
        Json = json;
      }
      catch (Exception ex)
      {
        Log.Exception("Load failed: " + path, ex, GetType());
      }
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

    private string GetPath() => Path.Combine(Settings.StorageDirectoryPath, Settings.StorageFileName);
  }
}