using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browsers;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Commands;

internal sealed partial class SearchWebCommand : InvokableCommand
{
    public WebSearchShortcutDataEntry Shortcut;
    private readonly string _query;
    private readonly BrowserExecutionInfo BrowserInfo;
    // private readonly SettingsManager _settingsManager;

    internal SearchWebCommand(WebSearchShortcutDataEntry shortcut, string query)
    {
        Name = StringFormatter.Format(Resources.SearchQuery_NameTemplate, new() { ["engine"] = shortcut.Name, ["query"] = query });
        Icon = Icons.Search;
        Shortcut = shortcut;
        BrowserInfo = new BrowserExecutionInfo(shortcut);
        _query = query;
        // _settingsManager = settingsManager;
    }

    public override CommandResult Invoke()
    {
        if (!ShellHelpers.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, WebSearchShortcutDataEntry.GetSearchUrl(Shortcut, _query)))
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
