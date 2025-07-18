using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WebSearchShortcut;

public sealed class Storage
{
  public List<WebSearchShortcutItem> Data { get; set; } = [];

  // private static readonly JsonSerializerOptions _jsonOptions = new()
  // {
  //   IncludeFields = true,
  // };

  public static Storage ReadFromFile(string path)
  {
    var data = new Storage();

    if (!File.Exists(path))
    {
      var defaultStorage = new Storage();
      defaultStorage.Data.AddRange([
        new WebSearchShortcutItem
          {
            Name = "Google",
            Url = "https://www.google.com/search?q=%s",
            SuggestionProvider = "Google",
          },
        new WebSearchShortcutItem
          {
            Name = "Bing",
            Url = "https://www.bing.com/search?q=%s",
            SuggestionProvider = "Bing",
          },
        new WebSearchShortcutItem
          {
            Name = "Youtube",
            Url = "https://www.youtube.com/results?search_query=%s",
            SuggestionProvider = "YouTube"
          },
      ]);
      WriteToFile(path, defaultStorage);
    }
    // if the file exists, load it and append the new item
    if (File.Exists(path))
    {
      var jsonStringReading = File.ReadAllText(path);

      if (!string.IsNullOrEmpty(jsonStringReading))
      {
        data = JsonSerializer.Deserialize(jsonStringReading, AppJsonSerializerContext.Default.Storage) ?? new Storage();
        if (data.Data.Any(item => string.IsNullOrWhiteSpace(item.Id)))
        {
          WriteToFile(path, data);
          data = ReadFromFile(path);
        }
      }
    }

    return data;
  }

  public static void WriteToFile(string path, Storage data)
  {
    foreach (var item in data.Data)
    {
      if (string.IsNullOrWhiteSpace(item.Id))
      {
        item.Id = GenerateNewId();
      }
    }

    var jsonString = JsonSerializer.Serialize(data, AppJsonSerializerContext.Default.Storage);

    File.WriteAllText(path, jsonString);
  }

  private static string GenerateNewId()
  {
    string prefix = Windows.ApplicationModel.Package.Current.Id.FamilyName;

    byte[] buffer = new byte[8];
    Random.Shared.NextBytes(buffer);
    ulong randomNumber = BitConverter.ToUInt64(buffer, 0);

    return $"{prefix}!App!ID{randomNumber}";
    }
}
