using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browsers;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Commands;

internal sealed partial class SearchWebCommand : InvokableCommand
{
    // private readonly SettingsManager _settingsManager;
    public WebSearchShortcutItem Item;
    public string Query { get; internal set; } = string.Empty;
    private readonly BrowserExecutionInfo BrowserInfo;

    internal SearchWebCommand(WebSearchShortcutItem item, string query)
    {
        Query = query;
        BrowserInfo = new BrowserExecutionInfo(item);
        Icon = new IconInfo("\uE721");
        Name = StringFormatter.Format(Resources.SearchItem_NameTemplate, new() { ["engine"] = item.Name, ["query"] = query });
        Item = item;
        // _settingsManager = settingsManager;
    }

    public override CommandResult Invoke()
    {
        if (!ShellHelpers.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, WebSearchShortcutItem.GetSearchUrl(Item, Query)))
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
