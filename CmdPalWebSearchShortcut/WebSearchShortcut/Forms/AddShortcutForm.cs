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
            "value": {{JsonSerializer.Serialize(name)}},
            "isRequired": true,
            "errorMessage": "Name is required"
        },
        {
            "type": "Input.Text",
            "style": "text",
            "id": "url",
            "value": {{JsonSerializer.Serialize(url)}},
            "label": "Url",
            "isRequired": true,
            "errorMessage": "Url is required"
        },
        {
            "type": "Input.ChoiceSet",
            "id": "suggestionProvider",
            "value": {{JsonSerializer.Serialize(suggestionProvider)}},
            "label": "SuggestionProvider",
            "isRequired": false,
            "choices": [
                {
                    "title": "None",
                    "value": ""
                },
                {{Suggestions.SuggestionProviders.Keys.Select(k => $$"""
                {
                    "title": {{JsonSerializer.Serialize(k)}},
                    "value": {{JsonSerializer.Serialize(k)}}
                }
                """).Aggregate((a, b) => a + "," + b)}}
            ]
        },
        {
            "type": "Input.Text",
            "style": "text",
            "id": "replaceWhitespace",
            "value": {{JsonSerializer.Serialize(replaceWhitespace)}},
            "label": "ReplaceWhitespace",
            "placeholder": "Specify which character(s) to replace a space"
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
                "replaceWhitespace": "replaceWhitespace"
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

        var updated = _item ?? new WebSearchShortcutItem();
        updated.Name = formName.ToString();
        updated.Url = formUrl.ToString();
        updated.SuggestionProvider = formSuggestionProvider.ToString();
        updated.ReplaceWhitespace = formReplaceWhitespace.ToString();

        AddedCommand?.Invoke(this, updated);
        return CommandResult.GoHome();
    }
}