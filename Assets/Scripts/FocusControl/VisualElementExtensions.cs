using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementExtensions
{
    public static IFocusableTarget AsFocusable(this VisualElement element)
    {
        return new VisualElementFocusable(element);
    }

    public static void SimulateSubmit(this VisualElement element)
    {
        var submitEvent = NavigationSubmitEvent.GetPooled();
        submitEvent.target = element;
        element.SendEvent(submitEvent);
    }
}