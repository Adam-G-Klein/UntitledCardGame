using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementExtensions
{
    public static VisualElementFocusable AsFocusable(this VisualElement element)
    {
        if (!element.HasUserData<VisualElementFocusable>()) {
            element.SetUserData<VisualElementFocusable>(new VisualElementFocusable(element));
        }
        return element.GetUserData<VisualElementFocusable>();
    }

    public static void MakeFocusable(this VisualElement element) {
        element.AddToClassList("focusable");
        element.focusable = true;
    }

    public static void RegisterOnSelected(this VisualElement element, Action action) {
        element.RegisterCallback<ClickEvent>(evt => {
            action();
            if (evt.pointerType == "mouse") {
                element.schedule.Execute(() => element.Blur()).ExecuteLater(10);
            }
            OnSelectedTween(element);
        });
        element.RegisterCallback<NavigationSubmitEvent>(evt => {
            action();
            OnSelectedTween(element);
        });
    }

    public static void RegisterOnSelected(this VisualElement element, Action<ClickEvent> action) {
        element.RegisterCallback<ClickEvent>(evt => {
            action(evt);
            if (evt.pointerType == "mouse") {
                element.schedule.Execute(() => element.Blur()).ExecuteLater(10);
            }
        });
        element.RegisterCallback<NavigationSubmitEvent>(evt => {
            action(element.CreateFakeClickEvent());
            OnSelectedTween(element);
        });
    }

    private static void OnSelectedTween(VisualElement element) {
        LeanTween.value(1f, 0.92f, 0.05f)
            .setOnUpdate((float value) => {
                element.transform.scale = new Vector3(value, value, 1f);
            })
            .setOnComplete(() => {
                LeanTween.value(0.92f, 1f, 0.1f)
                    .setOnUpdate((float value) => {
                        element.transform.scale = new Vector3(value, value, 1f);
                    });
            });
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

    public static ClickEvent CreateFakeClickEvent(this VisualElement element) {
        var clickEvent = ClickEvent.GetPooled();
        clickEvent.target = element;
        return clickEvent;
    }

    private static UserDataWrapper GetOrCreateWrapper(this VisualElement element)
    {
        if (element.userData is not UserDataWrapper wrapper)
        {
            wrapper = new UserDataWrapper();
            element.userData = wrapper;
        }

        return wrapper;
    }

    public static void SetUserData<T>(this VisualElement element, T value)
    {
        element.GetOrCreateWrapper().Set(value);
    }

    public static T GetUserData<T>(this VisualElement element) where T : class
    {
        return element.GetOrCreateWrapper().Get<T>();
    }

    public static bool HasUserData<T>(this VisualElement element) where T : class
    {
        return element.userData is UserDataWrapper wrapper && wrapper.Has<T>();
    }

    public static void DoForAllChildren(this VisualElement element, Action<VisualElement> action) {
        foreach (VisualElement child in element.Children()){
            action(child);
        }
    }
}