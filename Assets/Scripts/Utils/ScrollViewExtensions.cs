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

    // This is just here to interface with user data nicely since user data stores only one object per type,
    // and in the crazy case that we want to store TWO separate List<Coroutine> objects in user data
    private class ScrollingCoroutines {
        public List<Coroutine> coroutines;

        public ScrollingCoroutines() {
            coroutines = new List<Coroutine>();
        }
    }
}