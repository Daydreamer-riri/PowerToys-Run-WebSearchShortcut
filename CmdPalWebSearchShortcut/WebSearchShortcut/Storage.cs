using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut;

public sealed class Storage
{
    public List<WebSearchShortcutDataEntry> Data { get; set; } = [];

    public static Storage ReadFromFile(string path)
    {
        var data = new Storage();

        if (!File.Exists(path))
        {
            var defaultStorage = new Storage();
            defaultStorage.Data.AddRange([
                new WebSearchShortcutDataEntry
                {
                    Name = "Google",
                    Url = "https://www.google.com/search?q=%s",
                    SuggestionProvider = "Google",
                },
                new WebSearchShortcutDataEntry
                {
                    Name = "Bing",
                    Url = "https://www.bing.com/search?q=%s",
                    SuggestionProvider = "Bing",
                },
                new WebSearchShortcutDataEntry
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
            }
        }

        return data;
    }

    public static void WriteToFile(string path, Storage data)
    {
        var jsonString = JsonPrettyFormatter.ToPrettyJson(data, AppJsonSerializerContext.Default.Storage);

        File.WriteAllText(path, jsonString);
    }
}
