using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FocusManager : GenericSingleton<FocusManager>, IControlsReceiver
{
    private List<IFocusableTarget> focusableTargets = new List<IFocusableTarget>();
    private HashSet<IFocusableTarget> disabledFocusableTargets = new HashSet<IFocusableTarget>();
    private IFocusableTarget currentFocus = null;

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
        if (target == currentFocus) {
            currentFocus.Unfocus();
        }
    }

    public void DisableFocusableTarget(IFocusableTarget target) {
        disabledFocusableTargets.Add(target);
        if (target.Equals(currentFocus)) {
            Debug.Log("Disabling current focus target");
            currentFocus.Unfocus();
        }
    }

    public void EnableFocusableTarget(IFocusableTarget target) {
        disabledFocusableTargets.Remove(target);
    }

    public void SetFocus(IFocusableTarget target) {
        Unfocus();
        this.currentFocus = target;
        this.currentFocus.Focus();
    }

    public void Unfocus() {
        if (this.currentFocus != null) this.currentFocus.Unfocus();
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
        Vector2 currentCenter = GetCenterForFocusable(currentFocus);

        IFocusableTarget best = null;
        float bestScore = float.MaxValue;

        foreach (IFocusableTarget target in focusableTargets) {
            if (target == currentFocus || disabledFocusableTargets.Contains(target)) continue;

            Vector2 candidateCenter = GetCenterForFocusable(target);
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
            currentFocus.Unfocus();
            currentFocus = best;
            currentFocus.Focus();
        }
    }

    private Vector2 GetCenterForFocusable(IFocusableTarget target) {
        Vector2 center;
        if (target is VisualElementFocusable veFocusable) {
            center = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(veFocusable.GetVisualElement());
        } else {
            GameObjectFocusable goFocusable = target as GameObjectFocusable;
            center = goFocusable.transform.position;
        }
        return center;
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (FocusableProcessedAction(action)) return;

        if (currentFocus == null) {
            foreach (IFocusableTarget target in focusableTargets) {
                if (!disabledFocusableTargets.Contains(target)) {
                    currentFocus = target;
                    target.Focus();
                    return;
                }
            }
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
        }
    }

    public void UnregisterFocusables(UIDocument uiDoc) {
        VisualElement root = uiDoc.rootVisualElement;
        foreach (var el in root.Query<VisualElement>(className: "focusable").ToList())
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
}
