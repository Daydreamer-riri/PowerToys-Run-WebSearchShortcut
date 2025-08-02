using System.Collections.Generic;

namespace WebSearchShortcut.Helpers;

/// <summary>
/// A simple string formatter utility that replaces placeholders in template strings.
/// This class replaces the SmartFormat dependency with a lightweight alternative.
/// </summary>
public static class StringFormatter
{
    /// <summary>
    /// Formats a template string by replacing placeholders with provided values.
    /// </summary>
    /// <param name="template">The template string containing placeholders like {key}</param>
    /// <param name="values">Dictionary of key-value pairs for placeholder replacement</param>
    /// <returns>The formatted string with placeholders replaced</returns>
    public static string Format(string template, Dictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        var result = template;
        foreach (var kvp in values)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
        }
        return result;
    }
}
