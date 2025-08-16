using System.Text.Json.Serialization;

namespace WebSearchShortcut;

[JsonSourceGenerationOptions(IncludeFields = true)]
[JsonSerializable(typeof(Storage))]
[JsonSerializable(typeof(WebSearchShortcutDataEntry))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
