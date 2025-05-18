using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class CompanionManagementSlotView {
    public VisualElement root;
    public CompanionManagementView companionManagementView;
    public VisualElementFocusable veFocusable;

    private bool blocked = false;
    private Color defaultBackgroundColor;
    private Color highlightedColor;
    private Color blockedColor;
    
    public CompanionManagementSlotView(
            VisualElement slotRoot,
            Color defaultBackgroundColor,
            Color highlightedColor,
            Color blockedColor) {
        this.root = slotRoot;
        this.companionManagementView = null;
        this.defaultBackgroundColor = defaultBackgroundColor;
        this.highlightedColor = highlightedColor;
        this.blockedColor = blockedColor;

        this.veFocusable = root.AsFocusable();
        FocusManager.Instance.RegisterFocusableTarget(veFocusable);
        FocusManager.Instance.DisableFocusableTarget(veFocusable);

        root.RegisterCallback<NavigationSubmitEvent>(NavigationSubmitHandler);
        veFocusable.SetInputAction(GFGInputAction.SELECT_DOWN, () => SelectDownHandler());
        veFocusable.additionalFocusAction += FocusHandler;
        veFocusable.additionalUnfocusAction += UnfocusHandler;

        veFocusable.SetInputAction(GFGInputAction.VIEW_DECK, ViewDeckHandler);
        veFocusable.SetInputAction(GFGInputAction.SELL_COMPANION, SellCompanionHandler);
    }

    public void SetBlocked() {
        this.blocked = true;
        root.style.backgroundColor = blockedColor;
        FocusManager.Instance.DisableFocusableTarget(this.veFocusable);
    }

    public void InsertCompanion(CompanionManagementView companionManagementView) {
        this.companionManagementView = companionManagementView;
        root.Add(companionManagementView.container);
        FocusManager.Instance.EnableFocusableTarget(this.veFocusable);
    }

    public void RemoveCompanion() {
        this.companionManagementView = null;
        root.Clear();
    }

    public void Reset() {
        this.companionManagementView = null;
        root.Clear();
        root.style.backgroundColor = defaultBackgroundColor;
        this.blocked = false;
        FocusManager.Instance.DisableFocusableTarget(this.veFocusable);
    }

    public void SetHighlighted() {
        root.style.backgroundColor = highlightedColor;
    }

    public void SetNotHighlighted() {
        root.style.backgroundColor = defaultBackgroundColor;
    }

    public bool IsBlocked() {
        return blocked;
    }

    public bool IsEmpty() {
        return companionManagementView == null;
    }

    public bool ContainsPosition(Vector2 position) {
        return root.worldBound.Contains(position);
    }

    public void NavigationSubmitHandler(NavigationSubmitEvent evt) {
        if (companionManagementView != null) {
            companionManagementView.CompanionManagementNonMouseSelect();
        }
    }

    public void SelectDownHandler() {
        if (companionManagementView != null) {
            companionManagementView.CompanionManagementOnPointerDown(root.CreateFakePointerDownEvent(), false);
        }
    }

    public void FocusHandler() {
        if (companionManagementView != null) {
            companionManagementView.CompanionManagementOnPointerEnter(null);
        }
    }

    public void UnfocusHandler() {
        if (companionManagementView != null) {
            companionManagementView.CompanionManagementOnUnfocus();
        }
    }

    private void ViewDeckHandler() {
        if (companionManagementView != null) {
            companionManagementView.ViewDeckButtonOnClick();
        }
    }

    private void SellCompanionHandler() {
        if (companionManagementView != null) {
            companionManagementView.SellCompanionButtonOnClick();
        }
    }

    public static void MakeAllFocusable(List<CompanionManagementSlotView> slots) {
        slots.ForEach((slot) => {
            if (!slot.IsBlocked()) FocusManager.Instance.EnableFocusableTarget(slot.veFocusable);
        });
    }

    public static void ResetFocusable(List<CompanionManagementSlotView> slots) {
        slots.ForEach((slot) => {
            if (!slot.IsEmpty() && !slot.IsBlocked()) {
                FocusManager.Instance.EnableFocusableTarget(slot.veFocusable);
            } else {
                FocusManager.Instance.DisableFocusableTarget(slot.veFocusable);
            }
        });
    }
}