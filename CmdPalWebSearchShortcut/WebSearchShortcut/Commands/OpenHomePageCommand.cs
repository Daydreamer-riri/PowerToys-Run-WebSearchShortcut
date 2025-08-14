using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browsers;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Commands;

internal sealed partial class OpenHomePageCommand : InvokableCommand
{
    public WebSearchShortcutDataEntry Shortcut;
    private readonly BrowserExecutionInfo _browserInfo;

    internal OpenHomePageCommand(WebSearchShortcutDataEntry shortcut)
    {
        Name = StringFormatter.Format(Resources.OpenHomePage_NameTemplate, new() { ["engine"] = shortcut.Name });
        Icon = Icons.Search;
        Shortcut = shortcut;
        _browserInfo = new BrowserExecutionInfo(shortcut);
    }

    public override CommandResult Invoke()
    {
        if (!ShellHelpers.OpenCommandInShell(_browserInfo.Path, _browserInfo.ArgumentsPattern, WebSearchShortcutDataEntry.GetHomePageUrl(Shortcut)))
        {
            // TODO GH# 138 --> actually display feedback from the extension somewhere.
            return CommandResult.KeepOpen();
        }

        return CommandResult.Dismiss();
    }
}
