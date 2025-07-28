// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace WebSearchShortcut.Browsers;

/// <summary>
/// Contains information (e.g. path to executable, name...) about the default browser.
/// </summary>
public static class DefaultBrowserProvider
{
  /// <summary>Gets the MS Edge browse.</summary>
  private static readonly BrowserInfo MSEdgeBrowser = new(
      "MSEdgeHTM",
      "Microsoft Edge",
      System.IO.Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
          @"Microsoft\Edge\Application\msedge.exe"
      ),
      "--single-argument %1"
  );

  private static readonly Lock _updateLock = new();

  /// <summary>Gets the path to default browser's executable.</summary>
  public static string? Path { get; private set; }

  /// <summary>Gets <see cref="Path"/> since the icon is embedded in the executable.</summary>
  public static string? IconPath => Path;

  /// <summary>Gets the user-friendly name of the default browser.</summary>
  public static string? Name { get; private set; }

  /// <summary>Gets the command line pattern of the default browser.</summary>
  public static string? ArgumentsPattern { get; private set; }

  public static bool IsDefaultBrowserSet => !string.IsNullOrEmpty(Path);

  public const long UpdateTimeout = 300;

  private static long _lastUpdateTickCount = -UpdateTimeout;

  private static bool _updatedOnce;
  private static bool _errorLogged;

  /// <summary>
  /// Updates only if at least more than 300ms has passed since the last update, to avoid multiple calls to <see cref="Update"/>.
  /// (because of multiple plugins calling update at the same time.)
  /// </summary>
  public static void UpdateIfTimePassed()
  {
    var curTickCount = Environment.TickCount64;
    if (curTickCount - _lastUpdateTickCount >= UpdateTimeout)
    {
      _lastUpdateTickCount = curTickCount;
      Update();
    }
  }

  private static string? GetRegistryValue(string registryLocation, string? valueName)
  {
    return Registry.GetValue(registryLocation, valueName, null) as string;
  }
  private static string? GetDefaultBrowserProgId()
  {
    return GetRegistryValue(
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoiceLatest",
            "ProgId")
        ?? GetRegistryValue(
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice",
            "ProgId");
  }

  /// <summary>
  /// Consider using <see cref="UpdateIfTimePassed"/> to avoid updating multiple times.
  /// (because of multiple plugins calling update at the same time.)
  /// </summary>
  public static void Update()
  {
    lock (_updateLock)
    {
      if (!_updatedOnce)
      {
        // Log.Info("I've tried updating the chosen Web Browser info at least once.", typeof(DefaultBrowserInfo));
        _updatedOnce = true;
      }

      try
      {
        string progId = GetDefaultBrowserProgId()!;
        BrowserInfo browser = ProgIdBrowserResolver.GetBrowserInfoFromProgId(progId);
        Apply(browser);
      }
      catch (Exception)
      {
        // Fallback to MS Edge
        Apply(MSEdgeBrowser);

        if (!_errorLogged)
        {
          // Log.Exception("Exception when retrieving browser path/name. Path and Name are set to use Microsoft Edge.", e, typeof(DefaultBrowserInfo));
          _errorLogged = true;
        }
      }
    }
  }
  private static void Apply(BrowserInfo browser)
  {
    Path = browser.Path;
    Name = browser.Name;
    ArgumentsPattern = browser.ArgumentsPattern;
  }
}
