using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

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

        TemplateJson = $$"""
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
        {
            "type": "Input.Text",
            "style": "text",
            "id": "name",
            "label": "Name",
            "value": {{JsonSerializer.Serialize(name, AppJsonSerializerContext.Default.String)}},
            "isRequired": true,
            "errorMessage": "Name is required"
        },
        {
            "type": "Input.Text",
            "style": "text",
            "id": "url",
            "value": {{JsonSerializer.Serialize(url, AppJsonSerializerContext.Default.String)}},
            "label": "Url",
            "isRequired": true,
            "errorMessage": "Url is required"
        },
        {
            "type": "Input.ChoiceSet",
            "id": "suggestionProvider",
            "value": {{JsonSerializer.Serialize(suggestionProvider, AppJsonSerializerContext.Default.String)}},
            "label": "SuggestionProvider",
            "isRequired": false,
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
            "errorMessage": "//"
        },
        {
            "type": "Input.Text",
            "style": "text",
            "id": "replaceWhitespace",
            "value": {{JsonSerializer.Serialize(replaceWhitespace, AppJsonSerializerContext.Default.String)}},
            "label": "ReplaceWhitespace",
            "placeholder": "Specify which character(s) to replace a space",
            "errorMessage": "//"
        },
        {
            "type": "Input.Text",
            "style": "text",
            "id": "homePage",
            "value": {{JsonSerializer.Serialize(homePage, AppJsonSerializerContext.Default.String)}},
            "label": "HomePage",
            "placeholder": "Optional: custom home page URL (empty = use domain)",
            "isRequired": false,
            "errorMessage": "//"
        },
        {
            "type": "ActionSet",
            "actions": [
                {
                    "type": "Action.ToggleVisibility",
                    "title": "Show Advanced Browser Options",
                    "targetElements": [
                        "advancedBrowserSettings"
                    ]
                }
            ]
        },
        {
            "type": "Container",
            "id": "advancedBrowserSettings",
            "isVisible": false,
            "items": [
                {
                    "type": "Input.Text",
                    "style": "text",
                    "id": "browserPath",
                    "value": {{JsonSerializer.Serialize(browserPath, AppJsonSerializerContext.Default.String)}},
                    "label": "BrowserPath",
                    "placeholder": "Custom browser path (empty = default browser)",
                    "isRequired": false
                },
                {
                    "type": "Input.Text",
                    "style": "text",
                    "id": "browserArgs",
                    "value": {{JsonSerializer.Serialize(browserArgs, AppJsonSerializerContext.Default.String)}},
                    "label": "BrowserArgs",
                    "placeholder": "Optional launch arguments, use %1 for URL",
                    "isRequired": false
                }
            ]
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
