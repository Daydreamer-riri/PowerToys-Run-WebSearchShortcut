using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.System;

namespace WebSearchShortcut;

public partial class UrlCommand : InvokableCommand
{
    public string Url { get; }

    public UrlCommand(WebSearchShortcutItem data)
        : this(data.Name, data.Url)
    {
    }

    public UrlCommand(string name, string url)
    {
        Name = name;
        Url = url;
        Icon = new IconInfo(IconFromUrl(Url));
    }

    public override CommandResult Invoke()
    {
        var target = Url;
        try
        {
            var uri = GetUri(target);
            if (uri != null)
            {
                _ = Launcher.LaunchUriAsync(uri);
            }
            else
            {
                // throw new UriFormatException("The provided URL is not valid.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error launching URL: {ex.Message}");
        }

        return CommandResult.Dismiss();
    }

    internal static Uri? GetUri(string url)
    {
        Uri? uri;
        if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
            if (!Uri.TryCreate("https://" + url, UriKind.Absolute, out uri))
            {
                return null;
            }
        }

        return uri;
    }

    internal static string IconFromUrl(string url)
    {
        var placeholderIndex = url.IndexOf('{');
        var baseString = placeholderIndex > 0 ? url.Substring(0, placeholderIndex) : url;
        try
        {
            var uri = GetUri(baseString);
            if (uri != null)
            {
                var hostname = uri.Host;
                var faviconUrl = $"{uri.Scheme}://{hostname}/favicon.ico";
                return faviconUrl;
            }
        }
        catch (UriFormatException)
        {
            // return "🔗";
        }

        return "🔗";
    }
}