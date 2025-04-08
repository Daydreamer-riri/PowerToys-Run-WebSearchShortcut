
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace WebSearchShortcut;

internal sealed partial class AddShortcutPage : ContentPage
{
    private readonly AddShortcutForm _addShortcut;

    internal event TypedEventHandler<object, WebSearchShortcutItem>? AddedCommand
    {
      add => _addShortcut.AddedCommand += value;
      remove => _addShortcut.AddedCommand -= value;
    }

    public override IContent[] GetContent() => [_addShortcut];

    public AddShortcutPage(WebSearchShortcutItem? item)
    {
        var name = item?.Name ?? string.Empty;
        var url = item?.Url ?? string.Empty;
        var isAdd = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url);

        _addShortcut = new AddShortcutForm(item);
        // Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = isAdd ? "Add Shortcut" : "Edit Shortcut";
        Name = isAdd ? "Add Shortcut" : "Edit Shortcut";
    }
}
