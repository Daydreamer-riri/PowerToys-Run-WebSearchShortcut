using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace WebSearchShortcut.Browsers;

public static class ProgIdBrowserResolver
{
  public static BrowserInfo GetBrowserInfoFromProgId(string progId)
  {
    string name = GetBrowserName(progId)
        ?? throw new InvalidOperationException($"No browser name found for ProgId: {progId}");

    string commandPattern = GetBrowserCommandPattern(progId)
        ?? throw new InvalidOperationException($"No browser command found for ProgId: {progId}");

    commandPattern = FixFirefoxCommandIfNeeded(commandPattern);

    var (path, args) = ParseCommandPattern(commandPattern);

    if (!File.Exists(path) && !Uri.TryCreate(path, UriKind.Absolute, out _))
    {
      throw new ArgumentException($"Invalid browser path from ProgId: {progId} → {path}");
    }

    return new BrowserInfo
    {
      Name = name,
      Path = path,
      ArgumentsPattern = args
    };
  }

  private static string? GetRegistryValue(string registryLocation, string? valueName)
  {
    return Registry.GetValue(registryLocation, valueName, null) as string;
  }

  private static string GetIndirectString(string str)
  {
    var stringBuilder = new StringBuilder(128);
    unsafe
    {
      var buffer = stackalloc char[128];
      var capacity = 128;
      void* reserved = null;

      // S_OK == 0
      if (SHLoadIndirectString(
              str,
              buffer,
              (uint)capacity,
              ref reserved)
          == 0)
      {
        return new string(buffer);
      }
    }

    throw new ArgumentNullException(nameof(str), "Could not load indirect string.");

    // Add this P/Invoke definition at the end of the class
    [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    static extern unsafe int SHLoadIndirectString(
        string pszSource,
        char* pszOutBuf,
        uint cchOutBuf,
        ref void* ppvReserved);
  }

  private static string? GetBrowserName(string progId)
  {
    var appName = GetRegistryValue($@"HKEY_CLASSES_ROOT\{progId}\Application", "ApplicationName")
        ?? GetRegistryValue($@"HKEY_CLASSES_ROOT\{progId}", "FriendlyTypeName");

    if (appName != null && appName.StartsWith('@'))
    {
      appName = GetIndirectString(appName);
    }

    return appName?
        .Replace("URL", "", StringComparison.OrdinalIgnoreCase)
        .Replace("HTML", "", StringComparison.OrdinalIgnoreCase)
        .Replace("Document", "", StringComparison.OrdinalIgnoreCase)
        .Replace("Web", "", StringComparison.OrdinalIgnoreCase)
        .TrimEnd();
  }

  private static string? GetBrowserCommandPattern(string progId)
  {
    var commandPattern = GetRegistryValue($@"HKEY_CLASSES_ROOT\{progId}\shell\open\command", null);

    if (string.IsNullOrEmpty(commandPattern))
    {
      throw new ArgumentException("Default browser program command is not specified.");
    }

    if (commandPattern.StartsWith('@'))
    {
      commandPattern = GetIndirectString(commandPattern);
    }

    return commandPattern;
  }

  private static string FixFirefoxCommandIfNeeded(string commandPattern)
  {
    // HACK: for firefox installed through Microsoft store
    // When installed through Microsoft Firefox the commandPattern does not have
    // quotes for the path. As the Program Files does have a space
    // the extracted path would be invalid, here we add the quotes to fix it
    const string FirefoxExecutableName = "firefox.exe";
    if (commandPattern.Contains(FirefoxExecutableName) &&
        commandPattern.Contains(@"\WindowsApps\") &&
        !commandPattern.StartsWith('\"'))
    {
      var pathEndIndex = commandPattern.IndexOf(FirefoxExecutableName, StringComparison.Ordinal) + FirefoxExecutableName.Length;
      commandPattern = commandPattern.Insert(pathEndIndex, "\"");
      commandPattern = commandPattern.Insert(0, "\"");
    }
    return commandPattern;
  }

  private static (string path, string args) ParseCommandPattern(string commandPattern)
  {
    if (commandPattern.StartsWith('\"'))
    {
      var endQuoteIndex = commandPattern.IndexOf('\"', 1);
      if (endQuoteIndex != -1)
      {
        var path = commandPattern.Substring(1, endQuoteIndex - 1);
        var args = commandPattern.Substring(endQuoteIndex + 1).Trim();
        return (path, args);
      }
    }
    else
    {
      var spaceIndex = commandPattern.IndexOf(' ');
      if (spaceIndex != -1)
      {
        var path = commandPattern.Substring(0, spaceIndex);
        var args = commandPattern.Substring(spaceIndex + 1).Trim();
        return (path, args);
      }
    }

    throw new InvalidOperationException($"Cannot parse command pattern into path and arguments: '{commandPattern}'");
  }
}
