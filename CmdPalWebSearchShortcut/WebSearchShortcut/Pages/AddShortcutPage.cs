using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut;

internal sealed partial class AddShortcutPage : ContentPage
{
    private readonly AddShortcutForm _addShortcut;

    public AddShortcutPage(WebSearchShortcutDataEntry? shortcut)
    {
        var name = shortcut?.Name ?? string.Empty;
        var url = shortcut?.Url ?? string.Empty;
        var isAdd = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url);

        _addShortcut = new AddShortcutForm(shortcut);
        Icon = IconHelpers.FromRelativePath("Assets\\SearchAdd.png");
        Title = isAdd ? Resources.AddShortcut_AddTitle : Resources.SearchShortcut_EditTitle;
        Name = isAdd ? Resources.AddShortcut_AddName : Resources.SearchShortcut_EditName;
    }

    internal event TypedEventHandler<object, WebSearchShortcutDataEntry>? AddedCommand
    {
        add => _addShortcut.AddedCommand += value;
        remove => _addShortcut.AddedCommand -= value;
    }

    public override IContent[] GetContent() => [_addShortcut];
}
