using UnityEngine;
using UnityEngine.UIElements;

public class IconVisualElement : IIconChange
{
    private VisualElement ve;
    private GFGInputAction action;

    public IconVisualElement(VisualElement ve) {
        this.ve = ve;
    }

    public void SetIcon(GFGInputAction action, Sprite sprite)
    {
        this.action = action;
        SetIcon(sprite);
    }

    public GFGInputAction GetAction()
    {
        return action;
    }

    public void SetIcon(Sprite sprite)
    {
        ve.style.backgroundImage = new StyleBackground(sprite);
    }
}