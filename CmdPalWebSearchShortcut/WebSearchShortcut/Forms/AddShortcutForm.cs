using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;
using WebSearchShortcut.Browsers;

namespace WebSearchShortcut;

internal sealed partial class AddShortcutForm : FormContent
{
  internal event TypedEventHandler<object, WebSearchShortcutItem>? AddedCommand;

  private readonly WebSearchShortcutItem? _item;

  public AddShortcutForm(WebSearchShortcutItem? item)
  {
    _item = item;
    var name = _item?.Name ?? string.Empty;
    var url = _item?.Url ?? string.Empty;
    var suggestionProvider = _item?.SuggestionProvider ?? string.Empty;
    var replaceWhitespace = _item?.ReplaceWhitespace ?? string.Empty;
    var homePage = _item?.HomePage ?? string.Empty;
    var browserPath = _item?.BrowserPath ?? string.Empty;
    var browserArgs = _item?.BrowserArgs ?? string.Empty;

    string browserChoices = BrowserDiscovery.GetAllInstalledBrowsers()
        .Where(b => !string.IsNullOrWhiteSpace(b.Path))
        .Select(b => $$"""
            {
                "title": {{JsonSerializer.Serialize(b.Name, AppJsonSerializerContext.Default.String)}},
                "value": {{JsonSerializer.Serialize(b.Path, AppJsonSerializerContext.Default.String)}}
            }
            """)
        .Aggregate((a, b) => a + "," + b);

    TemplateJson = $$"""
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
        {
            "id": "name",
            "type": "Input.Text",
            "label": "Name",
            "value": {{JsonSerializer.Serialize(name, AppJsonSerializerContext.Default.String)}},
            "isRequired": true,
            "errorMessage": "Name is required"
        },
        {
            "id": "url",
            "type": "Input.Text",
            "style": "Url",
            "label": "URL",
            "placeholder": "Use %s as the placeholder for search terms",
            "value": {{JsonSerializer.Serialize(url, AppJsonSerializerContext.Default.String)}},
            "isRequired": true,
            "errorMessage": "URL is required"
        },
        {
            "id": "suggestionProvider",
            "type": "Input.ChoiceSet",
            "label": "Suggestion Provider",
            "placeholder": "Invalid Suggestion Provider",
            "choices": [
                {
                    "title": "None",
                    "value": ""
                },
                {{Suggestions.SuggestionProviders.Keys.Select(k => $$"""
                {
                    "title": {{JsonSerializer.Serialize(k, AppJsonSerializerContext.Default.String)}},
                    "value": {{JsonSerializer.Serialize(k, AppJsonSerializerContext.Default.String)}}
                }
                """).Aggregate((a, b) => a + "," + b)}}
            ],
            "value": {{JsonSerializer.Serialize(suggestionProvider, AppJsonSerializerContext.Default.String)}}
        },
        {
            "type": "Input.Text",
            "style": "text",
            "id": "replaceWhitespace",
            "label": "Replace Whitespace",
            "placeholder": "Specify which character(s) to replace a space",
            "value": {{JsonSerializer.Serialize(replaceWhitespace, AppJsonSerializerContext.Default.String)}}
        },
        {
            "type": "Input.Text",
            "style": "text",
            "id": "homePage",
            "label": "Homepage",
            "placeholder": "Optional: custom home page URL (empty = use domain)",
            "value": {{JsonSerializer.Serialize(homePage, AppJsonSerializerContext.Default.String)}}
        },
        {
            "id": "browserPath",
            "type": "Input.ChoiceSet",
            "label": "Browser",
            "placeholder": {{JsonSerializer.Serialize(browserPath, AppJsonSerializerContext.Default.String)}},
            "choices": [
                {
                    "title": "Default",
                    "value": ""
                },
                {{browserChoices}}
            ],
            "value": {{JsonSerializer.Serialize(browserPath, AppJsonSerializerContext.Default.String)}}
        },
        {
            "id": "browserArgs",
            "type": "Input.Text",
            "style": "text",
            "label": "Browser Arguments",
            "placeholder": "Optional launch arguments, use %1 for URL",
            "value": {{JsonSerializer.Serialize(browserArgs, AppJsonSerializerContext.Default.String)}}
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "title": "Save",
            "data": {
                "name": "name",
                "url": "url",
                "suggestionProvider": "suggestionProvider",
                "replaceWhitespace": "replaceWhitespace",
                "homePage": "homePage",
                "browserPath": "browserPath",
                "browserArgs": "browserArgs"
            }
        }
    ]
}
""";
  }

  public override CommandResult SubmitForm(string payload)
  {
    var formInput = JsonNode.Parse(payload);
    if (formInput == null)
    {
      return CommandResult.GoHome();
    }

    // get the name and url out of the values
    var formName = formInput["name"] ?? string.Empty;
    var formUrl = formInput["url"] ?? string.Empty;
    var formSuggestionProvider = formInput["suggestionProvider"] ?? string.Empty;
    var formReplaceWhitespace = formInput["replaceWhitespace"] ?? string.Empty;
    var formHomePage = formInput["homePage"] ?? string.Empty;
    var formBrowserPath = formInput["browserPath"] ?? string.Empty;
    var formBrowserArgs = formInput["browserArgs"] ?? string.Empty;

    var updated = _item ?? new WebSearchShortcutItem();
    updated.Name = formName.ToString();
    updated.Url = formUrl.ToString();
    updated.SuggestionProvider = formSuggestionProvider.ToString();
    updated.ReplaceWhitespace = formReplaceWhitespace.ToString();
    updated.HomePage = formHomePage.ToString();
    updated.BrowserPath = formBrowserPath.ToString();
    updated.BrowserArgs = formBrowserArgs.ToString();

    AddedCommand?.Invoke(this, updated);
    return CommandResult.GoHome();
  }
}
