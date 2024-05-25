using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Community.PowerToys.Run.Plugin.WebSearchShortcut.Properties;
using Microsoft.PowerToys.Settings.UI.Library;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut
{
  /// <summary>
  /// Plugin settings.
  /// </summary>
  public class WebSearchShortcutSettings
  {
    private string _storageFileName = WebSearchShortcutStorage.DefaultFileName;

    /// <summary>
    /// File store.
    /// </summary>
    public string StorageFileName
    {
      get => _storageFileName;
      set => _storageFileName = IsValidFileName(value) ? value : WebSearchShortcutStorage.DefaultFileName;
    }

    internal string StorageDirectoryPath { get; set; } = null!;

    internal IEnumerable<PluginAdditionalOption> GetAdditionalOptions()
    {
      return
      [
          new()
                {
                    Key = nameof(StorageFileName),
                    DisplayLabel = Resources.settings_storage_file_name,
                    DisplayDescription = StorageDirectoryPath,
                    PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
                    TextValue = StorageFileName,
                },
            ];
    }

    internal void SetAdditionalOptions(IEnumerable<PluginAdditionalOption> additionalOptions)
    {
      ArgumentNullException.ThrowIfNull(additionalOptions);

      var options = additionalOptions.ToList();
      StorageFileName = options.Find(x => x.Key == nameof(StorageFileName))?.TextValue ?? WebSearchShortcutStorage.DefaultFileName;
    }

    private static bool IsValidFileName(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return false;
      }

      return value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }
  }
}