using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.WebSearchShortcut.Models;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure.Storage;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut
{
  /// <summary>
  /// Main class of this plugin that implement all used interfaces.
  /// </summary>
  public class Main : IPlugin, ISettingProvider, IDisposable
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Main"/> class.
    /// </summary>
    public Main()
    {
      Storage = new PluginJsonStorage<WebSearchShortcutSettings>();
      Settings = Storage.Load();
      Settings.StorageDirectoryPath = Storage.DirectoryPath;
      WebSearchShortcutStorage = new WebSearchShortcutStorage(Settings);
    }

    internal Main(WebSearchShortcutSettings settings, IWebSearchShortcutStorage webSearchShortcutStorage)
    {
      Storage = new PluginJsonStorage<WebSearchShortcutSettings>();
      Settings = settings;
      WebSearchShortcutStorage = webSearchShortcutStorage;
    }
    /// <summary>
    /// ID of the plugin.
    /// </summary>
    public static string PluginID => "B5E595872B8068104D5AD6BBE39A6664";

    /// <summary>
    /// Name of the plugin.
    /// </summary>
    public string Name => "WebSearchShortcut";

    /// <summary>
    /// Description of the plugin.
    /// </summary>
    public string Description => "Count words and characters in text";

    /// <summary>
    /// Additional options for the plugin.
    /// </summary>
    public IEnumerable<PluginAdditionalOption> AdditionalOptions => Settings.GetAdditionalOptions();

    private PluginInitContext? Context { get; set; }

    private string? IconPath { get; set; }

    private bool Disposed { get; set; }

    private PluginJsonStorage<WebSearchShortcutSettings> Storage { get; }

    private WebSearchShortcutSettings Settings { get; }

    private IWebSearchShortcutStorage WebSearchShortcutStorage { get; }

    /// <summary>
    /// Return a filtered list, based on the given query.
    /// </summary>
    /// <param name="query">The query to filter the list.</param>
    /// <returns>A filtered list, can be empty when nothing was found.</returns>
    public List<Result> Query(Query query)
    {
      if (query?.Search is null)
      {
        return [];
      }

      var args = query.Search;

      if (args.Trim() == "!reload")
      {
        return
        [
          new()
          {
            Title = "Reload data",
            SubTitle = "Reloads the data from the file",
            IcoPath = IconPath,
            Action = _ =>
            {
              ReloadData();
              return true;
            },
          },
        ];
      }

      if (string.IsNullOrEmpty(args))
      {
        return WebSearchShortcutStorage.GetRecords().Select(GetResultForGetRecord).ToList();
      }

      var tokens = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
      if (tokens.Length == 1)
      {
        return WebSearchShortcutStorage.GetRecords(args).Select(GetResultForGetRecord).ToList() ?? [];
      }

      var item = WebSearchShortcutStorage.GetRecords(tokens[0]).ToList()[0];
      if (item is null)
      {
        return [];
      }
      return [
        new Result
          {
            QueryTextDisplay = args,
            IcoPath = IconPath,
            Title = $"{item.Name}: {tokens[1]}",
            SubTitle = $"Search for {item.Name} with {tokens[1]}",
            // Action = _ =>
            // {
            //   Context!.API.ChangeQuery(item.Url);
            //   return false;
            // },
            Score = 1000,
            ContextData = item,
          }
      ];

      Result GetResultForGetRecord(Item record) => new()
      {
        QueryTextDisplay = args,
        IcoPath = IconPath,
        Title = record.Name,
        SubTitle = record.Url,
        // ToolTipData = new ToolTipData("Get", $"Key: {record.Key}\nValue: {record.Value}\nCreated: {record.Created}\nUpdated: {record.Updated}"),
        Action = _ =>
        {
          var newQuery = string.IsNullOrWhiteSpace(query.ActionKeyword)
            ? $"{record.Name} "
            : $"{query.ActionKeyword} {record.Name} ";
          Context!.API.ChangeQuery(newQuery, true);
          return false;
        },
        ContextData = record,
      };
    }

    /// <summary>
    /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
    public void Init(PluginInitContext context)
    {
      Log.Info("Init", GetType());

      Context = context ?? throw new ArgumentNullException(nameof(context));
      Context.API.ThemeChanged += OnThemeChanged;
      UpdateIconPath(Context.API.GetCurrentTheme());
    }

    // /// <summary>
    // /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
    // /// </summary>
    // /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
    // /// <returns>A list context menu entries.</returns>
    // public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    // {
    //   Log.Info("LoadContextMenus", GetType());

    //   if (selectedResult?.ContextData is (int words, TimeSpan transcription))
    //   {
    //     return
    //     [
    //         new ContextMenuResult
    //                 {
    //                     PluginName = Name,
    //                     Title = "Copy (Enter)",
    //                     FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
    //                     Glyph = "\xE8C8", // Copy
    //                     AcceleratorKey = Key.Enter,
    //                     Action = _ => CopyToClipboard(words.ToString()),
    //                 },
    //                 new ContextMenuResult
    //                 {
    //                     PluginName = Name,
    //                     Title = "Copy time (Ctrl+Enter)",
    //                     FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
    //                     Glyph = "\xE916", // Stopwatch
    //                     AcceleratorKey = Key.Enter,
    //                     AcceleratorModifiers = ModifierKeys.Control,
    //                     Action = _ => CopyToClipboard(transcription.ToString()),
    //                 },
    //             ];
    //   }

    //   if (selectedResult?.ContextData is int characters)
    //   {
    //     return
    //     [
    //         new ContextMenuResult
    //                 {
    //                     PluginName = Name,
    //                     Title = "Copy (Enter)",
    //                     FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
    //                     Glyph = "\xE8C8", // Copy
    //                     AcceleratorKey = Key.Enter,
    //                     Action = _ => CopyToClipboard(characters.ToString()),
    //                 },
    //             ];
    //   }

    //   return [];
    // }

    /// <summary>
    /// Creates setting panel.
    /// </summary>
    /// <returns>The control.</returns>
    /// <exception cref="NotImplementedException">method is not implemented.</exception>
    public Control CreateSettingPanel() => throw new NotImplementedException();

    /// <summary>
    /// Updates settings.
    /// </summary>
    /// <param name="settings">The plugin settings.</param>
    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
      Log.Info("UpdateSettings", GetType());

      // CountSpaces = settings.AdditionalOptions.SingleOrDefault(x => x.Key == nameof(CountSpaces))?.Value ?? false;
    }

    public void ReloadData()
    {
      WebSearchShortcutStorage.Load();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      Log.Info("Dispose", GetType());

      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
    /// </summary>
    /// <param name="disposing">Indicate that the plugin is disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (Disposed || !disposing)
      {
        return;
      }

      if (Context?.API != null)
      {
        Context.API.ThemeChanged -= OnThemeChanged;
      }

      Disposed = true;
    }

    private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? Context?.CurrentPluginMetadata.IcoPathLight : Context?.CurrentPluginMetadata.IcoPathDark;

    private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

    private static bool CopyToClipboard(string? value)
    {
      if (value != null)
      {
        Clipboard.SetText(value);
      }

      return true;
    }
  }
}