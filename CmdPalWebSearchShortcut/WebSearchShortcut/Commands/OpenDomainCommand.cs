
using Microsoft.CommandPalette.Extensions.Toolkit;

using BrowserInfo = WebSearchShortcut.Helpers.DefaultBrowserInfo;

namespace WebSearchShortcut.Commands;

internal sealed partial class OpenDomainCommand : InvokableCommand
{
  // private readonly SettingsManager _settingsManager;

  public WebSearchShortcutItem Item;

  internal OpenDomainCommand(WebSearchShortcutItem item)
  {
    BrowserInfo.UpdateIfTimePassed();
    Icon = new IconInfo("\uE721");
    Name = $"Open {item.Name}";
    Item = item;
    // Icon = IconHelpers.FromRelativePath("Assets\\WebSearch.png");
    // Name = Properties.Resources.open_in_default_browser;
    // _settingsManager = settingsManager;
  }

  public override CommandResult Invoke()
  {
    if (!ShellHelpers.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, $"{Item.Domain}"))
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