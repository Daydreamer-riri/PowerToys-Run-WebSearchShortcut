using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;

namespace WebSearchShortcut.Browsers;

public static class BrowserDiscovery
{
    private static readonly Lock _loadLock = new();
    private static bool _isLoaded;
    private static BrowserInfo[] _cachedBrowsers = [];

    public static BrowserInfo[] GetAllInstalledBrowsers()
    {
        if (_isLoaded) return _cachedBrowsers;

        lock (_loadLock)
        {
            if (!_isLoaded)
            {
                _cachedBrowsers = LoadInstalledBrowsers();
                _isLoaded = true;
            }
        }

        return _cachedBrowsers;
    }

    public static void Reload()
    {
        lock (_loadLock)
        {
            _cachedBrowsers = LoadInstalledBrowsers();
            _isLoaded = true;
        }
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
