// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Constants;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Services;

namespace WebSearchShortcut;

public partial class WebSearchShortcutCommandsProvider : CommandProvider
{
    private readonly List<ICommandItem> _commands;
    private readonly AddShortcutPage _addNewCommand = new(null);

    private Storage? _storage;


    public WebSearchShortcutCommandsProvider()
    {
        DisplayName = Resources.WebSearchShortcutCommandsProvider_DisplayName;
        Icon = IconHelpers.FromRelativePath("Assets\\Search.png");
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

        var url = await IconService.UpdateIconUrlAsync(item);
        SaveAndUpdateCommands();
        ExtensionHost.LogMessage($"Updating icon URL for bookmark ({item.Name},{item.Url}) to {url}");
    }

    private void Edit_AddedCommand(object sender, WebSearchShortcutItem args)
    {
        ExtensionHost.LogMessage($"Edited bookmark ({args.Name},{args.Url})");
        UpdateIconUrl(args);

        SaveAndUpdateCommands();
    }

    private void LoadCommands()
    {
        List<CommandItem> collected = [];
        collected.Add(new CommandItem(_addNewCommand));

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
        try
        {

            var jsonFile = StateJsonPath();
            _storage = Storage.ReadFromFile(jsonFile);
        }
        catch (Exception ex)
        {
            // debug log error
            Debug.WriteLine($"Error loading commands: {ex.Message}");
        }
    }

    private CommandItem ShortcutToCommandItem(WebSearchShortcutItem item)
    {
        ICommand command = new SearchPage(item);
        var listItem = new CommandItem(command) { Icon = command.Icon };
        List<CommandContextItem> contextMenu = [];

        if (command is SearchPage searchPage)
        {
            listItem.Subtitle = StringFormatter.Format(Resources.WebSearchShortcutCommandsProvider_CommandItemSubtitle, new() { ["engine"] = item.Name });
        }

        var edit = new AddShortcutPage(item) { Icon = Icons.Edit };
        edit.AddedCommand += Edit_AddedCommand;
        contextMenu.Add(new CommandContextItem(edit));

        var delete = new CommandContextItem(
            title: Resources.WebSearchShortcutCommandsProvider_CommandItemDeleteTitle,
            name: Resources.WebSearchShortcutCommandsProvider_CommandItemDeleteName,
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
            Icon = Icons.Delete,
        };
        contextMenu.Add(delete);

        listItem.MoreCommands = [.. contextMenu];

        return listItem;
    }

    private void SaveAndUpdateCommands()
    {
        if (_storage != null)
        {
            var jsonPath = StateJsonPath();
            Storage.WriteToFile(jsonPath, _storage);
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

    internal static string StateJsonPath()
    {
        var directory = Utilities.BaseSettingsPath("WebSearchShortcut");
        Directory.CreateDirectory(directory);

        // now, the state is just next to the exe
        return Path.Combine(directory, "WebSearchShortcut.json");
    }
}
