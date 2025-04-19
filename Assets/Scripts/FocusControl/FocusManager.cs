using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;

public class FocusManager : GenericSingleton<FocusManager>, IControlsReceiver
{
    private List<IFocusableTarget> focusableTargets = new List<IFocusableTarget>();
    private Dictionary<string, List<IFocusableTarget>> stashedFocusables = new Dictionary<string, List<IFocusableTarget>>();
    private HashSet<IFocusableTarget> disabledFocusableTargets = new HashSet<IFocusableTarget>();
    private IFocusableTarget currentFocus = null;

    public delegate void FocusableDelegate(IFocusableTarget focusable);
    public FocusableDelegate onFocusDelegate;
    public FocusableDelegate onUnfocusDelegate;

    // Own script's internal setup
    void Awake() {
    }

    void Start() {
        ControlsManager.Instance.RegisterControlsReceiver(this);
    }

    [ContextMenu("Focus")]
    private void TestFocus() {
        currentFocus = focusableTargets[0];
        currentFocus.Focus();
    }

    [ContextMenu("Unfocus")]
    private void TestUnfocus() {
        currentFocus.Unfocus();
    }

    public void RegisterFocusableTarget(IFocusableTarget target) {
        if (focusableTargets.Contains(target)) return;
        focusableTargets.Add(target);
    }

    public void UnregisterFocusableTarget(IFocusableTarget target) {
        focusableTargets.Remove(target);
        disabledFocusableTargets.Remove(target);
        if (target == currentFocus) {
            Unfocus();
            currentFocus = null;
        }
    }

    public void UnregisterAll() {
        List<IFocusableTarget> focusablesToRemove = new List<IFocusableTarget>(focusableTargets);
        foreach (IFocusableTarget target in focusablesToRemove) {
            UnregisterFocusableTarget(target);
        }
    }

    public void DisableFocusableTarget(IFocusableTarget target) {
        disabledFocusableTargets.Add(target);
        if (target.Equals(currentFocus)) {
            Debug.Log("Disabling current focus target");
            Unfocus();
            currentFocus = null;
        }
    }

    public void EnableFocusableTarget(IFocusableTarget target) {
        disabledFocusableTargets.Remove(target);
    }

    public void StashFocusables(string stashedBy) {
        List<IFocusableTarget> newStashedFocusables = new List<IFocusableTarget>();
        foreach (IFocusableTarget target in focusableTargets) {
            if (!disabledFocusableTargets.Contains(target)) {
                newStashedFocusables.Add(target);
                disabledFocusableTargets.Add(target);
                if (target.Equals(currentFocus)) {
                    Debug.Log("Disabling current focus target");
                    Unfocus();
                    currentFocus = null;
                }
            }
        }
        stashedFocusables[stashedBy] = newStashedFocusables;
    }

    public void UnstashFocusables(string stashedBy) {
        if (!stashedFocusables.ContainsKey(stashedBy)) {
            Debug.LogError("No key found to unstash focusables: " + stashedBy);
            return;
        }

        Debug.Log(disabledFocusableTargets.Count);
        List<IFocusableTarget> focusablesToUnstash = stashedFocusables[stashedBy];
        foreach (IFocusableTarget target in focusablesToUnstash) {
            if (disabledFocusableTargets.Contains(target)) {
                disabledFocusableTargets.Remove(target);
            }
        }
        Debug.Log(disabledFocusableTargets.Count);
        stashedFocusables.Remove(stashedBy);
    }

    public void SetFocus(IFocusableTarget target) {
        Unfocus();
        this.currentFocus = target;
        this.currentFocus.Focus();
    }

    public void Unfocus() {
        if (this.currentFocus == null) return;
        this.currentFocus.Unfocus();
        onUnfocusDelegate?.Invoke(currentFocus);
    }

    public void SetFocusNextFrame(IFocusableTarget target) {
        StartCoroutine(FocusNextFrame(target));
    }

    private IEnumerator FocusNextFrame(IFocusableTarget target)
    {
        yield return new WaitForEndOfFrame(); // wait one frame
        currentFocus = target;
        currentFocus.Focus();
    }

    private void MoveFocus(Vector2 direction) {
        Debug.Log("FocusManager MoveFocus " + direction.ToString());
        Vector2 currentCenter = currentFocus.GetPosition();

        IFocusableTarget best = null;
        float bestScore = float.MaxValue;

        foreach (IFocusableTarget target in focusableTargets) {
            if (target == currentFocus || disabledFocusableTargets.Contains(target)) continue;

            Vector2 candidateCenter = target.GetPosition();
            Vector2 toCandidate = (candidateCenter - currentCenter).normalized;

            float dot = Vector2.Dot(direction, toCandidate);

            // Only consider if directionally aligned (within ~70° cone)
            if (dot < 0.01f) continue;

            float distance = Vector2.Distance(currentCenter, candidateCenter);

            float alignment = Mathf.Clamp01(dot);
            float score = distance / (alignment); // add a little epsilon to avoid divide-by-zero

            if (score < bestScore) {
                bestScore = score;
                best = target;
            }
        }

        if (best != null) {
            Unfocus();
            currentFocus = best;
            currentFocus.Focus();
            onFocusDelegate?.Invoke(currentFocus);
        }
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (FocusableProcessedAction(action)) return;

        if (currentFocus == null) {
            FocusFirstEnabledFocusable();
            return;
        }

        switch(action) {
            case GFGInputAction.UP:
                MoveFocus(Vector2.up);
                break;
            case GFGInputAction.DOWN:
                MoveFocus(Vector2.down);
                break;
            case GFGInputAction.LEFT:
                MoveFocus(Vector2.left);
                break;
            case GFGInputAction.RIGHT:
                MoveFocus(Vector2.right); 
                break;
        }
    }

    private void FocusFirstEnabledFocusable() {
        Debug.Log("Focusing first focusable");
        foreach (IFocusableTarget target in focusableTargets) {
            if (!disabledFocusableTargets.Contains(target)) {
                currentFocus = target;
                target.Focus();
                onFocusDelegate?.Invoke(currentFocus);
                return;
            }
        }
    }

    // Returning false means we didn't process the input, so the default processing
    // should take over
    private bool FocusableProcessedAction(GFGInputAction action) {
        if (currentFocus == null) return false;
        return currentFocus.ProcessInput(action);
    }

    public void RegisterFocusables(UIDocument uiDoc) {
        VisualElement root = uiDoc.rootVisualElement;
        foreach (var el in root.Query<VisualElement>(className: "focusable").ToList())
        {
            RegisterFocusableTarget(el.AsFocusable());
            // This prevents UI elements from being focused by clicking on them
            // It's beahvior I don't think we want to have.
            el.RegisterCallback<PointerDownEvent>(evt => {
                evt.PreventDefault();
            });
        }
    }

    public void UnregisterFocusables(UIDocument uiDoc) {
        VisualElement root = uiDoc.rootVisualElement;
        foreach (var el in root.Query<VisualElement>(className: "focusable").ToList())
        {
            UnregisterFocusableTarget(el.AsFocusable());
        }
    }

    public void UnregisterFocusables(VisualElement element) {
        foreach (var el in element.Query<VisualElement>(className: "focusable").ToList())
        {
            UnregisterFocusableTarget(el.AsFocusable());
        }
    }

    public void DisableFocusables(UIDocument uiDoc) {
        VisualElement root = uiDoc.rootVisualElement;
        foreach (var el in root.Query<VisualElement>(className: "focusable").ToList())
        {
            DisableFocusableTarget(el.AsFocusable());
        }
    }

    public void EnableFocusables(UIDocument uiDoc) {
        VisualElement root = uiDoc.rootVisualElement;
        foreach (var el in root.Query<VisualElement>(className: "focusable").ToList())
        {
            EnableFocusableTarget(el.AsFocusable());
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        if (controlMethod == ControlsManager.ControlMethod.Mouse) {
            Unfocus();
            currentFocus = null;
        }
    }

    // I don't really like this logic living in this class, but I also don't really want
    // to put it in GetTargets. Idk man :shrug:
    public void StashFocusablesNotOfTargetType(List<Targetable.TargetType> types, string stashedBy) {
        List<IFocusableTarget> newStashedFocusables = new List<IFocusableTarget>();
        foreach (IFocusableTarget focusableTarget in focusableTargets) {
            Targetable.TargetType focusableTargetType = Targetable.TargetType.None;
            if (focusableTarget is VisualElementFocusable veFocusable) {
                focusableTargetType = veFocusable.GetTargetType();
            } else if (focusableTarget is GameObjectFocusable goFocusable) {
                focusableTargetType = goFocusable.GetComponent<Targetable>().targetType;
            }

            if (!types.Contains(focusableTargetType)) {
                if (!disabledFocusableTargets.Contains(focusableTarget)) {
                    newStashedFocusables.Add(focusableTarget);
                    disabledFocusableTargets.Add(focusableTarget);
                    if (focusableTarget.Equals(currentFocus)) {
                        Debug.Log("Disabling current focus target");
                        Unfocus();
                        currentFocus = null;
                    }
                }
            }
        }
        if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.KeyboardController)
            FocusFirstEnabledFocusable();
        stashedFocusables[stashedBy] = newStashedFocusables;
    }
}
