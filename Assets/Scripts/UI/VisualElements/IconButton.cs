using UnityEngine;
using UnityEngine.UIElements;

public class IconButton : Button
{
    private Image _iconImage;
    private Label _textLabel;

    public new class UxmlFactory : UxmlFactory<IconButton, UxmlTraits> { }

    public IconButton()
    {
        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;
        style.justifyContent = Justify.Center;

        _iconImage = new Image();
        _iconImage.name = "icon"; // So it’s accessible in UI Builder
        _iconImage.scaleMode = ScaleMode.ScaleAndCrop;
        _iconImage.AddToClassList("icon");

        _textLabel = new Label();
        _textLabel.AddToClassList("text");

        hierarchy.Add(_iconImage);
        hierarchy.Add(_textLabel);
    }

    public void SetIcon(Sprite sprite)
    {
        _iconImage.sprite = sprite;
    }

    public void SetIcon(Texture2D texture)
    {
        _iconImage.image = texture;
    }

    public override string text
    {
        get => _textLabel.text;
        set => _textLabel.text = value;
    }
}
