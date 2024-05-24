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

    public string[]? Urls { get; set; }

    public string? SuggestionProvider { get; set; }

    public bool? IsDefault { get; set; }

    public string? IconUrl { get; set; }

    private string? IconFileName { get; set; }
    public string GetIconFileName()
    {
      if (!string.IsNullOrEmpty(IconFileName))
      {
        return IconFileName;
      }
      var _FileName = Name ?? "";
      char[] invalidChars = [':', '/', '\\', '?', '*', '<', '>', '|'];
      foreach (var invalidChar in invalidChars)
      {
        _FileName = _FileName.Replace(invalidChar, '_');
      }
      IconFileName = _FileName;
      return IconFileName;
    }

    public string Domain {
      get
      {
        return new Uri(Url.Split(' ')[0].Split('?')[0]).GetLeftPart(UriPartial.Authority);
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
      string iconPath = Path.Combine(Main.PluginDirectory, "Images", "Icons", $"{GetIconFileName()}.png");
      if (!string.IsNullOrEmpty(IconPath) && File.Exists(iconPath))
      {
        IconPath = $@"Images\Icons\{GetIconFileName()}.png";
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
        IconPath = $@"Images\Icons\{GetIconFileName()}.png";
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
        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

        string faviconUrl = !string.IsNullOrEmpty(IconUrl) ? IconUrl : Domain + "/favicon.ico";
        HttpResponseMessage response = await client.GetAsync(faviconUrl);
        if (
          response.IsSuccessStatusCode
          && response.Content.Headers.ContentType?.MediaType != "text/html"
          )
        {
          return await response.Content.ReadAsByteArrayAsync();
        }
        HttpResponseMessage googleResponse = await client.GetAsync($"https://www.google.com/s2/favicons?sz=64&domain={Domain}");
        if (googleResponse.IsSuccessStatusCode)
        {
          return await googleResponse.Content.ReadAsByteArrayAsync();
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