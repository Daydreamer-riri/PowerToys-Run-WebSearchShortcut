using System;
using Microsoft.CommandPalette.Extensions.Toolkit;

using BrowserInfo = WebSearchShortcut.Helpers.DefaultBrowserInfo;

namespace WebSearchShortcut.Commands;

internal sealed partial class SearchWebCommand : InvokableCommand
{
  // private readonly SettingsManager _settingsManager;

  public string Arguments { get; internal set; } = string.Empty;

  internal SearchWebCommand(string arguments)
  {
    Arguments = arguments;
    BrowserInfo.UpdateIfTimePassed();
    Icon = new IconInfo("\uE721");
    // Icon = IconHelpers.FromRelativePath("Assets\\WebSearch.png");
    // Name = Properties.Resources.open_in_default_browser;
    // _settingsManager = settingsManager;
  }

  public override CommandResult Invoke()
  {
    if (!ShellHelpers.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, $"? {Arguments}"))
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