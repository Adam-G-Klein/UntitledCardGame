using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualElementFocusable : IFocusableTarget
{
    public Action additionalFocusAction = null;
    public Action additionalUnfocusAction = null;
    private VisualElement element;
    private Dictionary<GFGInputAction, Action> actionMap;
    private Targetable.TargetType optionalTargetType = Targetable.TargetType.None;

    public VisualElementFocusable(VisualElement element)
    {
        this.element = element;
        this.actionMap = new Dictionary<GFGInputAction, Action>();
        this.actionMap[GFGInputAction.SELECT] = () => element.SimulateSubmit();
        this.element.RegisterCallback<DetachFromPanelEvent>(evt => {
            FocusManager.Instance.UnregisterFocusableTarget(this);
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

    public Vector2 GetUIPosition()
    {
        return element.worldBound.center;
    }
}