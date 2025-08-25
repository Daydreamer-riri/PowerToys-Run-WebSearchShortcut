using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Windows.ApplicationModel;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut;

internal sealed class Storage
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

        // if the file exists, load the saved shortcuts
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

    private static bool EnsureIds(List<WebSearchShortcutDataEntry> shortcuts)
    {
        bool modified = false;
        HashSet<string> existingIds = [];

        foreach (var shortcut in shortcuts)
        {
            while (string.IsNullOrWhiteSpace(shortcut.Id) || existingIds.Contains(shortcut.Id))
            {
                modified = true;

                shortcut.Id = GenerateNewId();
            }
            existingIds.Add(shortcut.Id);
        }

        return modified;
    }

    private static string GenerateNewId()
    {
        string prefix = Package.Current.Id.FamilyName;

        byte[] buffer = new byte[8];
        Random.Shared.NextBytes(buffer);
        ulong randomNumber = BitConverter.ToUInt64(buffer, 0);

        return $"{prefix}!App!ID{randomNumber}";
    }
}
