using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut;

internal sealed partial class AddShortcutPage : ContentPage
{
    private readonly AddShortcutForm _addShortcut;

    public AddShortcutPage(WebSearchShortcutItem? item)
    {
        var name = item?.Name ?? string.Empty;
        var url = item?.Url ?? string.Empty;
        var isAdd = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url);

        _addShortcut = new AddShortcutForm(item);
        Icon = IconHelpers.FromRelativePath("Assets\\SearchAdd.png");
        Title = isAdd ? Resources.AddShortcut_AddTitle : Resources.SearchShortcut_EditTitle;
        Name = isAdd ? Resources.AddShortcut_AddName : Resources.SearchShortcut_EditName;
    }

    internal event TypedEventHandler<object, WebSearchShortcutItem>? AddedCommand
    {
        add => _addShortcut.AddedCommand += value;
        remove => _addShortcut.AddedCommand -= value;
    }

    public override IContent[] GetContent() => [_addShortcut];
}
