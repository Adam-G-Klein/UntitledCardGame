using UnityEngine;

public interface IFocusableTarget
{
    void Focus();
    void Unfocus();
    bool ProcessInput(GFGInputAction action);
    Vector2 GetWorldspacePosition();
    Vector2 GetUIPosition();
    bool IsOnScreen();
    bool CanFocusOffscreen();
    object GetCommonalityObject();
}