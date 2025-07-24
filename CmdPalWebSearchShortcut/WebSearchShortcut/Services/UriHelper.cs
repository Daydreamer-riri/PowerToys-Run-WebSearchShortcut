// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace WebSearchShortcut.Services;

/// <summary>
/// Provides URI-related utility methods
/// </summary>
public static class UriHelper
{
    /// <summary>
    /// Attempts to create a valid URI from a string, with automatic https:// prefix if needed
    /// </summary>
    /// <param name="url">The URL string to convert</param>
    /// <returns>A valid URI or null if the URL is invalid</returns>
    public static Uri? GetUri(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // Try to create URI as-is
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            return uri;
        }

        // Try with https:// prefix
        if (Uri.TryCreate("https://" + url, UriKind.Absolute, out uri))
        {
            return uri;
        }

        return null;
    }

    /// <summary>
    /// Checks if a URL string is valid
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <returns>True if the URL is valid, false otherwise</returns>
    public static bool IsValidUrl(string url)
    {
        return GetUri(url) != null;
    }

    /// <summary>
    /// Gets the domain from a URL string
    /// </summary>
    /// <param name="url">The URL to extract domain from</param>
    /// <returns>The domain or null if invalid</returns>
    public static string? GetDomain(string url)
    {
        var uri = GetUri(url);
        return uri?.Host;
    }
}
