using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browsers;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Commands;

internal sealed partial class SearchWebCommand : InvokableCommand
{
  // private readonly SettingsManager _settingsManager;
  public string Arguments { get; internal set; } = string.Empty;
  public WebSearchShortcutItem Item;
  private readonly BrowserExecutionInfo BrowserInfo;

  internal SearchWebCommand(string arguments, WebSearchShortcutItem item)
  {
    Arguments = arguments;
    BrowserInfo = new BrowserExecutionInfo(item);
    Icon = new IconInfo("\uE721");
    Name = StringFormatter.Format(Resources.SearchWebCommand_Name, new() { ["engine"] = item.Name, ["query"] = arguments });
    Item = item;
    // Icon = IconHelpers.FromRelativePath("Assets\\WebSearch.png");
    // Name = Properties.Resources.open_in_default_browser;
    // _settingsManager = settingsManager;
  }

  public override CommandResult Invoke()
  {
    if (!ShellHelpers.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, $"{WebSearchShortcutItem.GetSearchUrl(Item, Arguments)}"))
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
