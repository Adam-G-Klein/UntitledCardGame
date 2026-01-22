using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualElementFocusable : IFocusableTarget
{
    public Action additionalFocusAction = null;
    public Action additionalUnfocusAction = null;
    public bool canFocusOffscreen = false;
    public object commonalityObject = null;
    private VisualElement element;
    private Dictionary<GFGInputAction, Action> actionMap;
    private Targetable.TargetType optionalTargetType = Targetable.TargetType.None;

    public VisualElementFocusable(VisualElement element)
    {
        this.element = element;
        this.actionMap = new Dictionary<GFGInputAction, Action>();
        this.actionMap[GFGInputAction.SELECT] = () => element.SimulateSubmit();
        this.element.RegisterCallback<DetachFromPanelEvent>(evt => {
            FocusManager focusManager = FocusManager.CheckInstance;
            if (focusManager) focusManager.UnregisterFocusableTarget(this);
        });
    }

    public override bool Equals(object obj)
    {
        if (obj is VisualElementFocusable other) {
            return this.element.Equals(other.element);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return this.element.GetHashCode();
    }

    public void Focus()
    {
        element.Focus();
        additionalFocusAction?.Invoke();
    }

    public void Unfocus()
    {
        element.Blur();
        additionalUnfocusAction?.Invoke();
    }

    // Returning true means the input was successfully processed
    public bool ProcessInput(GFGInputAction action) {
        if (!actionMap.ContainsKey(action)) return false;

        actionMap[action].Invoke();

        return true;
    }

    public void SetInputAction(GFGInputAction inputAction, Action action) {
        if (!this.actionMap.ContainsKey(inputAction)) {
            this.actionMap[inputAction] = null;
        }
        
        this.actionMap[inputAction] += action;
    }

    public void UnsetInputAction(GFGInputAction inputAction, Action action) {
        try {
            this.actionMap[inputAction] -= action;
        } catch (Exception e) {
            // idk man who cares
        }
    }

    public VisualElement GetVisualElement() {
        return element;
    }

    public void SetTargetType(Targetable.TargetType targetType) {
        optionalTargetType = targetType;
    }

    public Targetable.TargetType GetTargetType() {
        return optionalTargetType;
    }

    public Vector2 GetWorldspacePosition() {
        return UIDocumentGameObjectPlacer.GetWorldPositionFromElement(element);
    }

    public bool IsOnScreen() {
        // Get the world position of the element’s center
        var layout = element.worldBound;
        var center = new Vector2(layout.center.x, layout.center.y);

        // Get the root panel’s visible rect
        var panel = element.panel;
        if (panel == null)
            return false;

        var root = panel.visualTree;
        var screenRect = root.worldBound;

        // Check if the center is within the screen
        return screenRect.Contains(center);
    }

    public Vector2 GetUIPosition()
    {
        return element.worldBound.center;
    }

    public bool CanFocusOffscreen()
    {
        return canFocusOffscreen;
    }

    public object GetCommonalityObject() {
        return commonalityObject;
    }
}