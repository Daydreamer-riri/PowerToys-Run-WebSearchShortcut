using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut.Commands;

internal sealed partial class OpenHomePageCommand : InvokableCommand
{
  // private readonly SettingsManager _settingsManager;
  public WebSearchShortcutItem Item;

  internal OpenHomePageCommand(WebSearchShortcutItem item)
  {
    Icon = new IconInfo("\uE721");
    Name = StringFormatter.Format(Resources.OpenHomePageCommand_Name, new() { ["engine"] = item.Name });
    Item = item;
    // Icon = IconHelpers.FromRelativePath("Assets\\WebSearch.png");
    // Name = Properties.Resources.open_in_default_browser;
    // _settingsManager = settingsManager;
  }

  public override CommandResult Invoke()
  {
    if (!HomePageLauncher.OpenHomePageWithBrowser(Item))
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
