using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public static class ScrollViewExtensions {
    public static void StartScrolling(this ScrollView scrollView, float amount, int direction) {
        if (!scrollView.HasUserData<ScrollingCoroutines>()) {
            scrollView.SetUserData<ScrollingCoroutines>(new ScrollingCoroutines());
        }

        ScrollingCoroutines coroutines = scrollView.GetUserData<ScrollingCoroutines>();
        foreach (Coroutine coroutine in coroutines.coroutines) {
            CoroutineRunner.Instance.Stop(coroutine);
        }

        coroutines.coroutines.Add(CoroutineRunner.Instance.Run(ScrollCoroutine(scrollView, amount * direction)));
    }

    public static void StopScrolling(this ScrollView scrollView) {
        if (!scrollView.HasUserData<ScrollingCoroutines>()) {
            return;
        }

        ScrollingCoroutines coroutines = scrollView.GetUserData<ScrollingCoroutines>();
        foreach (Coroutine coroutine in coroutines.coroutines) {
            CoroutineRunner.Instance.Stop(coroutine);
        }
    }

    private static IEnumerator ScrollCoroutine(ScrollView scrollView, float amount) {
        while (true) {
            Scroller scroller = scrollView.verticalScroller;
            LeanTween.value(0, amount * scroller.highValue-scroller.lowValue, 0.05f)
                .setOnUpdate((float val) => {
                    scroller.value += val;
                });
            yield return new WaitForSeconds(0.05f);
        }
    }

    public static void ScrollToMakeElementVisible(this ScrollView scrollView, VisualElement target, float padding) {
        if (scrollView == null || target == null) return;

        // Viewport is what you can see (the "mask" area)
        var viewport = scrollView.contentViewport;
        // Content is what moves when you scroll
        var content = scrollView.contentContainer;
        // Target rect in WORLD space
        Rect targetWorld = target.worldBound;
        // Convert target rect into CONTENT local space
        Rect targetInContent = WorldRectToLocal(content, targetWorld);

        // Viewport rect in CONTENT local space:
        // We convert viewport's world rect into content local space as well.
        Rect viewportInContent = WorldRectToLocal(content, viewport.worldBound);

        Vector2 offset = scrollView.scrollOffset;

        // Vertical
        if (scrollView.mode == ScrollViewMode.VerticalAndHorizontal || scrollView.mode == ScrollViewMode.Vertical)
        {
            float topVisible = viewportInContent.yMin + padding;
            float bottomVisible = viewportInContent.yMax - padding;

            if (targetInContent.yMin < topVisible)
            {
                offset.y -= (topVisible - targetInContent.yMin);
            }
            else if (targetInContent.yMax > bottomVisible)
            {
                offset.y += (targetInContent.yMax - bottomVisible);
            }
        }

        // Horizontal
        if (scrollView.mode == ScrollViewMode.VerticalAndHorizontal || scrollView.mode == ScrollViewMode.Horizontal)
        {
            float leftVisible = viewportInContent.xMin + padding;
            float rightVisible = viewportInContent.xMax - padding;

            if (targetInContent.xMin < leftVisible)
            {
                offset.x -= (leftVisible - targetInContent.xMin);
            }
            else if (targetInContent.xMax > rightVisible)
            {
                offset.x += (targetInContent.xMax - rightVisible);
            }
        }

        // Clamp to valid scroll ranges
        offset.x = Mathf.Clamp(offset.x, 0f, scrollView.horizontalScroller.highValue);
        offset.y = Mathf.Clamp(offset.y, 0f, scrollView.verticalScroller.highValue);

        Debug.Log(offset);

        scrollView.horizontalScroller.value = offset.x;
        scrollView.verticalScroller.value = offset.y;
    }

    private static Rect WorldRectToLocal(VisualElement localTo, Rect worldRect)
    {
        Vector2 min = localTo.WorldToLocal(worldRect.min);
        Vector2 max = localTo.WorldToLocal(worldRect.max);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    // This is just here to interface with user data nicely since user data stores only one object per type,
    // and in the crazy case that we want to store TWO separate List<Coroutine> objects in user data
    private class ScrollingCoroutines {
        public List<Coroutine> coroutines;

        public ScrollingCoroutines() {
            coroutines = new List<Coroutine>();
        }
    }
}