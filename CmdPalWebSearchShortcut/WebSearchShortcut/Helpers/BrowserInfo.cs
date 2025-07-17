using System;

namespace WebSearchShortcut.Helpers;

public class BrowserInfo
{
    public string? Path { get; }
    public string? ArgumentsPattern { get; }

    public BrowserInfo(WebSearchShortcutItem item)
    {
        DefaultBrowserInfo.UpdateIfTimePassed();

        if (!string.IsNullOrWhiteSpace(item.BrowserArgs))
        {
            Path = !string.IsNullOrWhiteSpace(item.BrowserPath)
            ? item.BrowserPath
            : DefaultBrowserInfo.Path;

            var pattern = item.BrowserArgs.Trim();
            if (!pattern.Contains("%1", StringComparison.Ordinal))
            {
                pattern += " %1";
            }
            ArgumentsPattern = pattern;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(item.BrowserPath))
            {
                Path = item.BrowserPath;
                ArgumentsPattern = "%1";
            }
            else
            {
                Path = DefaultBrowserInfo.Path;
                ArgumentsPattern = DefaultBrowserInfo.ArgumentsPattern;
            }
        }
    }
}
