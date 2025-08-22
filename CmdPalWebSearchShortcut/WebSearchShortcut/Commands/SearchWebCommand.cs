using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browsers;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Commands;

internal sealed partial class SearchWebCommand : InvokableCommand
{
    public string Query { get; internal set; } = string.Empty;
    private readonly WebSearchShortcutDataEntry _shortcut;
    private readonly BrowserExecutionInfo _browserInfo;
    // private readonly SettingsManager _settingsManager;

    public new string Id
    {
        get => base.Id;
        set => base.Id = value;
    }

    public SearchWebCommand(WebSearchShortcutDataEntry shortcut, string query)
    {
        Name = StringFormatter.Format(Resources.SearchQuery_NameTemplate, new() { ["engine"] = shortcut.Name, ["query"] = query });
        Icon = Icons.Search;
        Query = query;
        _shortcut = shortcut;
        _browserInfo = new BrowserExecutionInfo(shortcut);
        // _settingsManager = settingsManager;
    }

    public override CommandResult Invoke()
    {
        if (!ShellHelpers.OpenCommandInShell(_browserInfo.Path, _browserInfo.ArgumentsPattern, WebSearchShortcutDataEntry.GetSearchUrl(_shortcut, Query)))
        {
            // TODO GH# 138 --> actually display feedback from the extension somewhere.
            return CommandResult.KeepOpen();
        }

        // if (_settingsManager.ShowHistory != Resources.history_none)
        // {
        //   _settingsManager.SaveHistory(new HistoryItem(Arguments, DateTime.Now));
        // }

        return CommandResult.Dismiss();
    }
}
