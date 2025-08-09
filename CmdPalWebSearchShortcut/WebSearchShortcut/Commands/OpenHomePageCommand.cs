using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browsers;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Commands;

internal sealed partial class OpenHomePageCommand : InvokableCommand
{
    public WebSearchShortcutItem Item;
    private readonly BrowserExecutionInfo BrowserInfo;

    internal OpenHomePageCommand(WebSearchShortcutItem item)
    {
        Name = StringFormatter.Format(Resources.OpenHomePage_NameTemplate, new() { ["engine"] = item.Name });
        Icon = new IconInfo("\uE721");
        Item = item;
        BrowserInfo = new BrowserExecutionInfo(item);
    }

    public override CommandResult Invoke()
    {
        if (!ShellHelpers.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, WebSearchShortcutItem.GetHomePageUrl(Item)))
        {
            // TODO GH# 138 --> actually display feedback from the extension somewhere.
            return CommandResult.KeepOpen();
        }

        return CommandResult.Dismiss();
    }
}
