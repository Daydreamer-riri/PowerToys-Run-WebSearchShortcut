using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;

namespace WebSearchShortcut.Browsers;

public static class BrowserDiscovery
{
    private static Lazy<BrowserInfo[]> _installedBrowsersCache = CreateInstalledBrowsersCache();
    private static Lazy<BrowserInfo[]> CreateInstalledBrowsersCache() =>
        new(LoadInstalledBrowsers, LazyThreadSafetyMode.ExecutionAndPublication);
    public static IReadOnlyCollection<BrowserInfo> GetAllInstalledBrowsers() => _installedBrowsersCache.Value;

    public static void Reload(bool warm = false)
    {
        var newCache = CreateInstalledBrowsersCache();

        Interlocked.Exchange(ref _installedBrowsersCache, newCache);

        if (warm)
            _ = newCache.Value;
    }

    private static BrowserInfo[] LoadInstalledBrowsers()
    {
        string[] progIds = GetAssociatedProgIds();
        List<BrowserInfo> result = [];

        foreach (var progId in progIds)
        {
            try
            {
                BrowserInfo info = ProgIdBrowserResolver.GetBrowserInfoFromProgId(progId);
                result.Add(info);
            }
            catch
            {
            }
        }

        return [.. result.OrderBy(b => b.Name, StringComparer.OrdinalIgnoreCase)];
    }

    private static string[] GetAssociatedProgIds()
    {
        HashSet<string> progIdSet = [];

        progIdSet.UnionWith(ScanProgIdsFromRegistry(
            Registry.LocalMachine,
            @"SOFTWARE\Clients\StartMenuInternet",
            @"Capabilities\URLAssociations")
        );

        progIdSet.UnionWith(ScanProgIdsFromRegistry(
            Registry.ClassesRoot,
            @"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages",
            @"App\Capabilities\URLAssociations")
        );

        progIdSet.UnionWith(ScanProgIdsFromRegistry(
            Registry.CurrentUser,
            @"SOFTWARE\Clients\StartMenuInternet",
            @"Capabilities\URLAssociations")
        );

        return [.. progIdSet];
    }

    private static HashSet<string> ScanProgIdsFromRegistry(RegistryKey baseKey, string rootSubKey, string subKeySuffix)
    {
        HashSet<string> progIds = [];

        using RegistryKey? root = baseKey.OpenSubKey(rootSubKey);
        if (root == null) return progIds;

        foreach (string subName in root.GetSubKeyNames())
        {
            using RegistryKey? browserKey = root.OpenSubKey(subName);
            if (browserKey == null) continue;

            using RegistryKey? urlAssocKey = browserKey.OpenSubKey(subKeySuffix);
            if (urlAssocKey == null) continue;

            string? progId = urlAssocKey.GetValue("https") as string
                 ?? urlAssocKey.GetValue("http") as string;

            if (!string.IsNullOrWhiteSpace(progId))
            {
                progIds.Add(progId.Trim());
            }
        }
        return progIds;
    }
}
