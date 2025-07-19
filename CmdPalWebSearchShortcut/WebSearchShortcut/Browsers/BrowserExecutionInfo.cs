using System;
using System.Linq;

namespace WebSearchShortcut.Browsers;

public class BrowserExecutionInfo
{
  public string? Path { get; }
  public string? ArgumentsPattern { get; }

  public BrowserExecutionInfo(WebSearchShortcutItem item)
  {
    DefaultBrowserProvider.UpdateIfTimePassed();

    Path = !string.IsNullOrWhiteSpace(item.BrowserPath)
           ? item.BrowserPath
           : DefaultBrowserProvider.Path;

    string? trimmedArgs;

    if (!string.IsNullOrWhiteSpace(item.BrowserArgs))
    {
      trimmedArgs = item.BrowserArgs.Trim();
    }
    else if (string.IsNullOrWhiteSpace(item.BrowserPath))
    {
      trimmedArgs = DefaultBrowserProvider.ArgumentsPattern;
    }
    else
    {
      trimmedArgs = BrowserDiscovery
                        .GetAllInstalledBrowsers()
                        .FirstOrDefault(b => string.Equals(b.Path, item.BrowserPath, StringComparison.OrdinalIgnoreCase))
                        ?.ArgumentsPattern.Trim();
    }

    trimmedArgs ??= string.Empty;

    ArgumentsPattern = trimmedArgs.Contains("%1", StringComparison.Ordinal)
                     ? trimmedArgs
                    : trimmedArgs + " %1";
    }
}
