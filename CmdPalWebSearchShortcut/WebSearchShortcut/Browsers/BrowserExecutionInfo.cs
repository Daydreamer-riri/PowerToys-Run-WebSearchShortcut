using System;
using System.Linq;

namespace WebSearchShortcut.Browsers;

internal sealed class BrowserExecutionInfo
{
    public string? Path { get; }
    public string? ArgumentsPattern { get; }

    public BrowserExecutionInfo(WebSearchShortcutDataEntry shortcut)
    {
        DefaultBrowserProvider.UpdateIfTimePassed();

        Path = !string.IsNullOrWhiteSpace(shortcut.BrowserPath)
               ? shortcut.BrowserPath
               : DefaultBrowserProvider.Path;

        string? trimmedArgs;

        if (!string.IsNullOrWhiteSpace(shortcut.BrowserArgs))
        {
            trimmedArgs = shortcut.BrowserArgs.Trim();
        }
        else if (string.IsNullOrWhiteSpace(shortcut.BrowserPath))
        {
            trimmedArgs = DefaultBrowserProvider.ArgumentsPattern;
        }
        else
        {
            trimmedArgs = BrowserDiscovery
                              .GetAllInstalledBrowsers()
                              .FirstOrDefault(b => string.Equals(b.Path, shortcut.BrowserPath, StringComparison.OrdinalIgnoreCase))
                              ?.ArgumentsPattern.Trim();
        }

        trimmedArgs ??= string.Empty;

        ArgumentsPattern = trimmedArgs.Contains("%1", StringComparison.Ordinal)
                         ? trimmedArgs
                        : trimmedArgs + " %1";
    }
}
