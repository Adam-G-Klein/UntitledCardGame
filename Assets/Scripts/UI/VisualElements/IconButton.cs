using UnityEngine;
using UnityEngine.UIElements;

public class IconButton : Button, IIconChange
{
    private readonly Image _icon;
    private GFGInputAction action;

    public new class UxmlFactory : UxmlFactory<IconButton, UxmlTraits> { }

    public IconButton()
    {
        AddToClassList("icon-button");

        _icon = new Image();
        _icon.AddToClassList("icon-button-icon");
        hierarchy.Insert(0, _icon);
    }

    public void SetIcon(Texture2D texture)
    {
        _icon.image = texture;
    }

    public void SetIcon(GFGInputAction action, Sprite sprite)
    {
        this.action = action;
        SetIcon(sprite);
    }

    public void SetIcon(Sprite sprite)
    {
        _icon.sprite = sprite;
        if (sprite == null) {
            AddToClassList("icon-button-no-icon");
        } else {
            RemoveFromClassList("icon-button-no-icon");
        }
    }

    public GFGInputAction GetAction() {
        return this.action;
    }
}
