// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut;

public partial class WebSearchShortcutCommandsProvider : CommandProvider
{
    private readonly List<ICommandItem> _commands;
    private readonly AddShortcutPage _addNewCommand = new(null);

    private Storage? _storage;

    public static IconInfo DeleteIcon { get; private set; } = new("\uE74D"); // Delete

    public static IconInfo EditIcon { get; private set; } = new("\uE70F"); // Edit


    public WebSearchShortcutCommandsProvider()
    {
      DisplayName = "WebSearchShortcut";
      Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
    _commands = [
        // new CommandItem(new WebSearchShortcutPage()) { Title = DisplayName },
          ];
      _addNewCommand.AddedCommand += AddNewCommand_AddedCommand;
    }

    private void AddNewCommand_AddedCommand(object sender, WebSearchShortcutItem args)
    {
        ExtensionHost.LogMessage($"Adding bookmark ({args.Name},{args.Url})");
        if (_storage != null)
        {
            _storage.Data.Add(args);
            UpdateIconUrl(args);
        }

        SaveAndUpdateCommands();
    }

    private async void UpdateIconUrl(WebSearchShortcutItem item)
    {
        if (_storage == null || !string.IsNullOrWhiteSpace(item.IconUrl))
        {
            return;
        }
        var url = await UrlCommand.IconFromUrlFallback(new Uri(item.Domain));
        var target = item;
        if (target != null)
        {
            target.IconUrl = url;
            SaveAndUpdateCommands();
            ExtensionHost.LogMessage($"Updating icon URL for bookmark ({item.Name},{item.Url}) to {url}");
        }
    }

    private void Edit_AddedCommand(object sender, WebSearchShortcutItem args)
  {
    ExtensionHost.LogMessage($"Edited bookmark ({args.Name},{args.Url})");

    SaveAndUpdateCommands();
  }

    private void LoadCommands()
    {
        List<CommandItem> collected = [];
        collected.Add(new CommandItem(_addNewCommand));
    collected.Add(new CommandItem(
        "update",
        "Update",
        "",
        () =>
        {
          ExtensionHost.LogMessage("Updating bookmarks...");
          // LoadBookmarksFromFile();
          LoadCommands();
          RaiseItemsChanged(0);
        }
      ));

        if (_storage == null)
        {
            LoadShortcutFromFile();
        }

        if (_storage != null)
        {
            collected.AddRange(_storage.Data.Select(ShortcutToCommandItem));
        }

        _commands.Clear();
        _commands.AddRange(collected);
    }

    private void LoadShortcutFromFile()
    {
        // var jsonPath = BookmarksCommandProvider.StateJsonPath();
        // _storage = Storage.ReadFromFile(jsonPath);
        _storage = Storage.ReadFromFile("");
    }

    private CommandItem ShortcutToCommandItem(WebSearchShortcutItem item)
  {
    ICommand command = new UrlCommand(item);
    var listItem = new CommandItem(command) { Icon = command.Icon };
    List<CommandContextItem> contextMenu = [];

    if (command is UrlCommand urlCommand)
    {
      listItem.Subtitle = urlCommand.Url;
    }

    var edit = new AddShortcutPage(item) { Icon = EditIcon };
    edit.AddedCommand += Edit_AddedCommand;
    contextMenu.Add(new CommandContextItem(edit));

    var delete = new CommandContextItem(
        // title: Resources.bookmarks_delete_title,
        // name: Resources.bookmarks_delete_name,
        title: "Delete",
        name: "Delete",
        action: () =>
        {
          if (_storage != null)
          {
            ExtensionHost.LogMessage($"Deleting bookmark ({item.Name},{item.Url})");

            _storage.Data.Remove(item);

            SaveAndUpdateCommands();
          }
        },
        result: CommandResult.KeepOpen())
    {
      IsCritical = true,
      Icon = DeleteIcon,
    };
    contextMenu.Add(delete);

    listItem.MoreCommands = [.. contextMenu];

    return listItem;
  }

    private void SaveAndUpdateCommands()
    {
        if (_storage != null)
        {
            // var jsonPath = BookmarksCommandProvider.StateJsonPath();
            // Bookmarks.WriteToFile(jsonPath, _bookmarks);
        }

        LoadCommands();
        RaiseItemsChanged(0);
    }

    public override ICommandItem[] TopLevelCommands()
    {
        if (_commands.Count == 0)
            {
                LoadCommands();
            }

        return [.. _commands];
    }

}
