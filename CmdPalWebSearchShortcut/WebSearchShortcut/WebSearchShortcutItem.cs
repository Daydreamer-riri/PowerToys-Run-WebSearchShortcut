
using System;
using System.Text.Json.Serialization;

namespace WebSearchShortcut
{
    public class WebSearchShortcutItem
    {
        public string Name { get; set; } = string.Empty;

        public string? Keyword { get; set; }

        public string Url { get; set; } = string.Empty;

        public string[]? Urls { get; set; }

        public string? SuggestionProvider { get; set; }

        public bool? IsDefault { get; set; }

        public string? IconUrl { get; set; }

        public string? BrowserPath { get; set; }

        [JsonIgnore]
        public string Domain
        {
            get
            {
                return new Uri(Url.Split(' ')[0].Split('?')[0]).GetLeftPart(UriPartial.Authority);
            }
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