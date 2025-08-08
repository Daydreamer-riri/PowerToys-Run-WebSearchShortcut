using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browsers;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Commands;

internal sealed partial class OpenHomePageCommand : InvokableCommand
{
    // private readonly SettingsManager _settingsManager;
    public WebSearchShortcutItem Item;
    private readonly BrowserExecutionInfo BrowserInfo;

    internal OpenHomePageCommand(WebSearchShortcutItem item)
    {
        Name = StringFormatter.Format(Resources.OpenHomePage_NameTemplate, new() { ["engine"] = item.Name });
        Icon = new IconInfo("\uE721");
        Item = item;
        BrowserInfo = new BrowserExecutionInfo(item);
        // Icon = IconHelpers.FromRelativePath("Assets\\WebSearch.png");
        // Name = Properties.Resources.open_in_default_browser;
        // _settingsManager = settingsManager;
    }

    public override CommandResult Invoke()
    {
        if (!ShellHelpers.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, WebSearchShortcutItem.GetHomePageUrl(Item)))
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
