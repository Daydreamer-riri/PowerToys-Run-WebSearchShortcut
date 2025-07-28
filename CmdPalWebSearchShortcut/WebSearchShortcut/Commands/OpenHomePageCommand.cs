using System.Globalization;
using System.Text;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut.Commands;

internal sealed partial class OpenHomePageCommand : InvokableCommand
{
  private static readonly CompositeFormat _nameFormat = CompositeFormat.Parse(Resources.OpenHomePageCommand_Name);
  // private readonly SettingsManager _settingsManager;
  public WebSearchShortcutItem Item;

  internal OpenHomePageCommand(WebSearchShortcutItem item)
  {
    Icon = new IconInfo("\uE721");
    Name = string.Format(CultureInfo.CurrentCulture, _nameFormat, item.Name);
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
