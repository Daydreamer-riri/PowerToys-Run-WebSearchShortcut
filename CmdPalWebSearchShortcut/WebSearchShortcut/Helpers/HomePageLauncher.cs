using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using BrowserInfo = WebSearchShortcut.Helpers.DefaultBrowserInfo;

namespace WebSearchShortcut.Helpers;

internal static class HomePageLauncher
{
    private static string GetHomePageUrl(WebSearchShortcutItem item)
    {
        return !string.IsNullOrWhiteSpace(item.HomePage)
            ? item.HomePage
            : item.Domain;
    }

    public static bool OpenHomePageWithBrowser(WebSearchShortcutItem item)
    {
        var homePageUrl = GetHomePageUrl(item);
        BrowserInfo.UpdateIfTimePassed();
        return ShellHelpers.OpenCommandInShell(
            BrowserInfo.Path,
            BrowserInfo.ArgumentsPattern,
            homePageUrl
        );
    }
}
