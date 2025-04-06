using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WebSearchShortcut;

public sealed class Storage
{
    public List<WebSearchShortcutItem> Data { get; set; } = [];

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        IncludeFields = true,
    };

    public static Storage ReadFromFile(string path)
    {
        var data = new Storage();

        // if the file exists, load it and append the new item
        // if (File.Exists(path))
        // {
        //     var jsonStringReading = File.ReadAllText(path);

        //     if (!string.IsNullOrEmpty(jsonStringReading))
        //     {
        //         data = JsonSerializer.Deserialize<Storage>(jsonStringReading, _jsonOptions) ?? new Storage();
        //     }
        // }

        return data;
    }

    public static void WriteToFile(string path, Storage data)
    {
        var jsonString = JsonSerializer.Serialize(data, _jsonOptions);

        // File.WriteAllText(BookmarksCommandProvider.StateJsonPath(), jsonString);
    }
}