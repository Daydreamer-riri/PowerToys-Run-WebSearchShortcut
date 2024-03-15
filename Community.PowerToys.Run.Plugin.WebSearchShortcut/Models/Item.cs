using System;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut.Models
{
  /// <summary>
  /// Key/value record.
  /// </summary>
  public class Item
  {
    /// <summary>
    /// The key.
    /// </summary>
    public string KeyWord { get; set; } = string.Empty;

    /// <summary>
    /// The value.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? IconPath { get; set; }

    /// <summary>
    /// When the record was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// When the record was updated.
    /// </summary>
    public DateTime? Updated { get; set; }
  }
}