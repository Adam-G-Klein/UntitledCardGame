using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasIconIndicator : MonoBehaviour, IIconChange
{
    public Image image;
    public GFGInputAction action;
    public Sprite defaultSprite;
    public TextMeshProUGUI tmp;

    public GFGInputAction GetAction()
    {
        return action;
    }

    public void SetIcon(Sprite sprite)
    {
        if (sprite != null) image.sprite = sprite;
        else image.sprite = defaultSprite;
    }

    void Start() {
        ControlsManager.Instance.RegisterIconChanger(this);
        SetIcon(ControlsManager.Instance.GetSpriteForGFGAction(action));
    }
}
