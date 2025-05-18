using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementExtensions
{
    public static VisualElementFocusable AsFocusable(this VisualElement element)
    {
        return new VisualElementFocusable(element);
    }

    public static void MakeFocusable(this VisualElement element) {
        element.AddToClassList("focusable");
        element.focusable = true;
    }

    public static void RegisterOnSelected(this VisualElement element, Action action) {
        element.RegisterCallback<ClickEvent>(evt => action());
        element.RegisterCallback<NavigationSubmitEvent>(evt => action());
    }

    public static void RegisterOnSelected(this VisualElement element, Action<ClickEvent> action) {
        element.RegisterCallback<ClickEvent>(evt => action(evt));
        element.RegisterCallback<NavigationSubmitEvent>(evt => action(null));
    }

    public static void SimulateSubmit(this VisualElement element)
    {
        var submitEvent = NavigationSubmitEvent.GetPooled();
        submitEvent.target = element;
        element.SendEvent(submitEvent);
    }

    public static PointerEnterEvent CreateFakePointerEnterEvent(this VisualElement element) {
        var pointerEnterEvent = PointerEnterEvent.GetPooled();
        pointerEnterEvent.target = element;
        return pointerEnterEvent;
    }

    public static PointerLeaveEvent CreateFakePointerLeaveEvent(this VisualElement element) {
        var pointerLeaveEvent = PointerLeaveEvent.GetPooled();
        pointerLeaveEvent.target = element;
        return pointerLeaveEvent;
    }

    public static PointerDownEvent CreateFakePointerDownEvent(this VisualElement element) {
        var pointerDownEvent = PointerDownEvent.GetPooled();
        pointerDownEvent.target = element;
        return pointerDownEvent;
    }
}