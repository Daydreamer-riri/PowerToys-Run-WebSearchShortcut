
using System;
using System.Net;
using System.Text.Json.Serialization;

namespace WebSearchShortcut
{
  public class WebSearchShortcutItem
  {
    public string Name { get; set; } = string.Empty;

    public string? Keyword { get; set; }

    public string Url { get; set; } = string.Empty;

    // public string[]? Urls { get; set; }

    public string? SuggestionProvider { get; set; }

    // public bool? IsDefault { get; set; }

    public string? IconUrl { get; set; }

    public string? BrowserPath { get; set; }

    public string? ReplaceWhitespace { get; set; }

    [JsonIgnore]
    public string Domain
    {
      get
      {
        return new Uri(Url.Split(' ')[0].Split('?')[0]).GetLeftPart(UriPartial.Authority);
      }
    }

    static string UrlEncode(WebSearchShortcutItem item, string search)
    {
      if (string.IsNullOrWhiteSpace(item.ReplaceWhitespace) || item.ReplaceWhitespace == " ")
      {
        return WebUtility.UrlEncode(search);
      }
      if (item.ReplaceWhitespace == "%20")
      {
        return WebUtility.UrlEncode(search).Replace("+", "%20");
      }
      return WebUtility.UrlEncode(search.Replace(" ", item.ReplaceWhitespace));
    }

    static public string GetSearchUrl(WebSearchShortcutItem item, string search)
    {
      string arguments = item.Url.Replace("%s", UrlEncode(item, search));
      return arguments;
    }

    private string? IconFileName { get; set; }

    // public string GetIconFileName()
    // {
    //     if (!string.IsNullOrEmpty(IconFileName))
    //     {
    //         return IconFileName;
    //     }
    //     var _FileName = Name ?? "";
    //     char[] invalidChars = [':', '/', '\\', '?', '*', '<', '>', '|'];
    //     foreach (var invalidChar in invalidChars)
    //     {
    //         _FileName = _FileName.Replace(invalidChar, '_');
    //     }
    //     IconFileName = _FileName;
    //     return IconFileName;
    // }
  }
};