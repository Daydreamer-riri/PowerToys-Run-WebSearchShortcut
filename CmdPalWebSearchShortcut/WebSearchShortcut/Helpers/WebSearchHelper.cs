using Microsoft.CommandPalette.Extensions.Toolkit;

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
        var browserInfo = new BrowserInfo(item);

        return ShellHelpers.OpenCommandInShell(
            browserInfo.Path,
            browserInfo.ArgumentsPattern,
            homePageUrl
        );
    }
}
