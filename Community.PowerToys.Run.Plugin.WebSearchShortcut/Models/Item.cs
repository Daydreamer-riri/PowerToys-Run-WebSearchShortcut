using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.WebSearchShortcut.Models
{
  /// <summary>
  /// Key/value record.
  /// </summary>
  public partial class Item
  {
    /// <summary>
    /// The value.
    /// </summary>
    public string? Name { get; set; }

    public string? Keyword { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? SuggestionProvider { get; set; }

    public bool? IsDefault { get; set; }

    public string Domain {
      get
      {
        return new Uri(Url.Split('?')[0]).GetLeftPart(UriPartial.Authority);
      }
    }

    public string? IconPath { get; set; }

    public async Task<bool> DownLoadIcon()
    {
      string iconDirectory = Path.Combine(Main.PluginDirectory, "Images", "Icons");
      if (!Directory.Exists(iconDirectory))
      {
        Directory.CreateDirectory(iconDirectory);
      }
      string iconPath = Path.Combine(Main.PluginDirectory, "Images", "Icons", $"{Name}.png");
      if (!string.IsNullOrEmpty(IconPath) && File.Exists(iconPath))
      {
        IconPath = $@"Images\Icons\{Name}.png";
        return false;
      }

      byte[] icon = await DownloadFaviconAsync();
      if (icon.Length == 0)
      {
        return false;
      }

      try
      {
        await File.WriteAllBytesAsync(iconPath, icon);
        IconPath = $@"Images\Icons\{Name}.png";
      }
      catch (Exception ex)
      {
        Log.Error($"Plugin: {Main.PluginName}\n{ex}", typeof(Item));
        return false;
      }
      return true;
    }

    private async Task<byte[]> DownloadFaviconAsync()
    {
      try
      {
        string faviconUrl = Domain + "/favicon.ico";
        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
        HttpResponseMessage response = await client.GetAsync(faviconUrl);
        if (response.IsSuccessStatusCode)
        {
          return await response.Content.ReadAsByteArrayAsync();
        }
        HttpResponseMessage domainResponse = await client.GetAsync(Domain);
        var html = await domainResponse.Content.ReadAsStringAsync();
        Regex regex = IconRegex();
        Match match = regex.Match(html);
        if (match.Success)
        {
          var iconUrl = match.Groups[1].Value;
          if (iconUrl.StartsWith("//"))
          {
            iconUrl = "http:" + iconUrl;
          }
          response = await client.GetAsync(iconUrl);
          if (response.IsSuccessStatusCode)
          {
            return await response.Content.ReadAsByteArrayAsync();
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error($"Plugin: {Main.PluginName}\n{ex}", typeof(Item));
      }
      return [];
    }

    [GeneratedRegex("<link.*?rel=\"(?:shortcut )?icon\".*?href=\"([^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex IconRegex();
  }
}