// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.Properties;

/// <summary>
/// Provides commonly used icons for the WebSearchShortcut application
/// </summary>
public static class Icons
{
    /// <summary>
    /// Edit icon (pencil)
    /// </summary>
    public static IconInfo Edit { get; } = new("\uE70F");

    /// <summary>
    /// Delete icon (trash can)
    /// </summary>
    public static IconInfo Delete { get; } = new("\uE74D");

    /// <summary>
    /// Default fallback icon for links
    /// </summary>
    public static IconInfo Link { get; } = new("🔗");

    /// <summary>
    /// Search icon
    /// </summary>
    public static IconInfo Search { get; } = new("\uE721");
}
