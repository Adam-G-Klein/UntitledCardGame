using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class VisualElementFocusable : IFocusableTarget
{
    public Action additionalFocusAction = null;
    public Action additionalUnfocusAction = null;
    private VisualElement element;
    private Dictionary<GFGInputAction, Action> actionMap;

    public VisualElementFocusable(VisualElement element)
    {
        this.element = element;
        this.actionMap = new Dictionary<GFGInputAction, Action>();
        this.actionMap[GFGInputAction.SELECT] = () => element.SimulateSubmit();
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
}