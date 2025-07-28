using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WebSearchShortcut.Helpers;

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

                bool modified = EnsureIds(data.Data);
                if (modified)
                {
                    WriteToFile(path, data);
                }
            }
        }

        return data;
    }

    public static void WriteToFile(string path, Storage data)
    {
        EnsureIds(data.Data);

        var jsonString = JsonPrettyFormatter.ToPrettyJson(data, AppJsonSerializerContext.Default.Storage);

        File.WriteAllText(path, jsonString);
    }

    private static bool EnsureIds(List<WebSearchShortcutItem> items)
    {
        bool modified = false;
        HashSet<string> existingIds = [];

        foreach (var item in items)
        {
            while (string.IsNullOrWhiteSpace(item.Id) || existingIds.Contains(item.Id))
            {
                modified = true;
                item.Id = GenerateNewId();
            }
            existingIds.Add(item.Id);
        }

        return modified;
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
