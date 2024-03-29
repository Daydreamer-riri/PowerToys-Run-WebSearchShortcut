﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.WebSearchShortcut.Models;
using Community.PowerToys.Run.Plugin.WebSearchShortcut.Suggestion;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure;
using Wox.Infrastructure.Storage;
using Wox.Plugin;
using Wox.Plugin.Logger;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut
{
  /// <summary>
  /// Main class of this plugin that implement all used interfaces.
  /// </summary>
  public class Main : IPlugin, IDelayedExecutionPlugin, IContextMenu, ISettingProvider, IDisposable
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

    public static string PluginName => "WebSearchShortcut";

    /// <summary>
    /// Name of the plugin.
    /// </summary>
    public string Name => "WebSearchShortcut";

    /// <summary>
    /// Description of the plugin.
    /// </summary>
    public string Description => "A simple plugin for select a specific search engine to perform searches.";

    /// <summary>
    /// Additional options for the plugin.
    /// </summary>
    public IEnumerable<PluginAdditionalOption> AdditionalOptions => Settings.GetAdditionalOptions();

    private PluginInitContext? Context { get; set; }

    public static string PluginDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

    public static Dictionary<string, string> IconPath { get; set; } = new()
        {
            { "Search", @"Images\Search.light.png" },
            { "Config", @"Images\Config.light.png" },
            { "Reload", @"Images\Reload.light.png" },
            { "Suggestion", @"Images\Suggestion.light.png" },
        };

    private bool Disposed { get; set; }

    private PluginJsonStorage<WebSearchShortcutSettings> Storage { get; }

    private WebSearchShortcutSettings Settings { get; }

    private IWebSearchShortcutStorage WebSearchShortcutStorage { get; }

    private Suggestions suggestion = new();

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

      if (args.Trim().Equals("!reload", StringComparison.OrdinalIgnoreCase))
      {
        return
        [
          new()
          {
            QueryTextDisplay = args,
            Title = "Reload",
            SubTitle = "Reload data from config file",
            IcoPath = IconPath["Reload"],
            Action = _ =>
            {
              ReloadData();
              return true;
            },
          },
        ];
      }

      if (args.Trim().Equals("!config", StringComparison.OrdinalIgnoreCase))
      {
        return
        [
          new()
          {
            QueryTextDisplay = args,
            Title = "Open Config File",
            SubTitle = "Open the config file in the default editor",
            IcoPath = IconPath["Config"],
            Action = _ =>
            {
              if (!Helper.OpenInShell(WebSearchShortcutStorage.GetPath()))
              {
                Log.Error($"Plugin: {PluginName}\nCannot open {WebSearchShortcutStorage.GetPath()}", typeof(Main));
                return false;
              }
              return true;
            },
            ContextData = WebSearchShortcutStorage.GetPath(),
          }
        ];
      }

      if (string.IsNullOrEmpty(args))
      {
        return WebSearchShortcutStorage.GetRecords().Select(GetResultForSelect).ToList();
      }

      var tokens = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
      if (tokens.Length == 1)
      {
        return WebSearchShortcutStorage.GetRecords(args).Select(GetResultForSelect).ToList() ?? [];
      }

      var item = WebSearchShortcutStorage.GetRecord(tokens[0]);
      if (item is null)
      {
        return [];
      }

      return [GetResultForSearch(item, tokens[1], query), .. SuggestionsCache];

      Result GetResultForSelect(Item item) => new()
      {
        QueryTextDisplay = args,
        IcoPath = item.IconPath ?? IconPath["Search"],
        Title = item.Name,
        SubTitle = $"Search using {item.Name}",
        Action = _ =>
        {
          var newQuery = string.IsNullOrWhiteSpace(query.ActionKeyword)
            ? $"{item.Name} "
            : $"{query.ActionKeyword} {item.Name} ";
          Context!.API.ChangeQuery(newQuery, true);
          return false;
        },
        ContextData = item,
      };
    }

    public List<Result> Query(Query query, bool delayedExecution)
    {
      if (query?.Search is null || !delayedExecution)
      {
        return ResetSuggestionsCache();
      }
      var tokens = query.Search.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
      if (tokens.Length == 1)
      {
        return ResetSuggestionsCache();
      }
      var item = WebSearchShortcutStorage.GetRecord(tokens[0]);
      if (item is null || string.IsNullOrEmpty(item.SuggestionProvider))
      {
        return ResetSuggestionsCache();
      }
      var suggestions = suggestion.QuerySuggestionsAsync(item.SuggestionProvider, tokens[1]).Result;
      if (suggestions.Count == 0)
      {
        return ResetSuggestionsCache();
      }
      var res = suggestions.Select(s => GetResultForSuggestion(item, s, query)).ToList();
      SuggestionsCache = [..res];

      res.Insert(0, GetResultForSearch(item, tokens[1], query));

      return res;

      List<Result> ResetSuggestionsCache()
      {
        SuggestionsCache = [];
        return [];
      }
    }

    private List<Result> SuggestionsCache { get; set; } = [];

    private static Result GetResultForSearch(Item item, string search, Query query)
    {
      string searchQuery = WebUtility.UrlEncode(search);
      string arguments = item.Url.Replace("%s", searchQuery);
      return new Result
      {
        QueryTextDisplay = query.Search,
        IcoPath = item.IconPath ?? IconPath["Search"],
        Title = $"{item.Name} ⏐  {search}",
        SubTitle = $"Search for {search} using {item.Name}",
        ProgramArguments = arguments,
        Action = _ => OpenInBrowser(arguments),
        Score = 1000,
        ToolTipData = new ToolTipData("Open", $"{arguments}"),
        ContextData = item,
      };
    }

    private static Result GetResultForSuggestion(Item item, SuggestionsItem suggest, Query query)
    {
      var search = query.Search.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1];
      return new Result
      {
        QueryTextDisplay = query.Search.Replace(search, suggest.Title),
        IcoPath = IconPath["Suggestion"],
        Title = suggest.Title,
        SubTitle = suggest.Description ?? $"Search for {suggest} using {item.Name}",
        Action = _ => OpenInBrowser(item.Url.Replace("%s", WebUtility.UrlEncode(suggest.Title))),
        ContextData = item,
        Score = 99,
      };
    }

    public List<ContextMenuResult> LoadContextMenus(Result result)
    {
      if (result?.ContextData is string)
      {
        return [
          new()
          {
            PluginName = PluginName,
            Title = "Open in Explorer (Ctrl + Enter)",
            Glyph = "\xe838",
            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
            Action = _ => Helper.OpenInShell(Settings.StorageDirectoryPath),
            AcceleratorKey = Key.Enter,
            AcceleratorModifiers = ModifierKeys.Control,
          }
        ];
      }
      if (result?.ContextData is null)
      {
        return [];
      }

      var item = (Item)result.ContextData;
      var domain = item.Domain;
      return [
        new()
        {
          PluginName = PluginName,
          Title = $"Open {item.Name} (Ctrl + Enter)",
          Glyph = "\xe8a7",
          FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
          Action = _ => OpenInBrowser(domain),
          AcceleratorKey = Key.Enter,
          AcceleratorModifiers = ModifierKeys.Control,
        },
      ];
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

    private static void UpdateIconPath(Theme theme)
    {
      bool isLightTheme = theme == Theme.Light || theme == Theme.HighContrastWhite;
      foreach (string key in IconPath.Keys)
      {
        IconPath[key] = IconPath[key].Replace(isLightTheme ? "dark" : "light",
                                              isLightTheme ? "light" : "dark");
      }
    }

    private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

    private static bool OpenInBrowser(string url)
    {
      if (!Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, url))
      {
        Log.Error($"Plugin: {PluginName}\nCannot open {BrowserInfo.Path} with arguments {BrowserInfo.ArgumentsPattern} {url}", typeof(Item));
        return false;
      }
      return true;
    }
  }
}