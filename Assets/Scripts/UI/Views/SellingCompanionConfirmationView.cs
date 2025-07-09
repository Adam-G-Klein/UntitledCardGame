using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.UIElements;

public class SellingCompanionConfirmationView {
    private VisualElement rootVisualElement;
    private ISellingCompanionConfirmationViewDelegate viewDelegate;
    private string originalSellingCompanionConfirmationText;
    private string originalSellingCompanionBreakdownText;

    private Button sellingYes;
    private Button sellingNo;

    private bool visible = false;

    public SellingCompanionConfirmationView(VisualElement rootVisualElement, ISellingCompanionConfirmationViewDelegate viewDelegate) {
        this.rootVisualElement = rootVisualElement;
        this.viewDelegate = viewDelegate;
        Setup();
    }

    private void Setup() {
        sellingYes = rootVisualElement.Q<Button>("selling-companion-confirmation-yes");
        sellingNo = rootVisualElement.Q<Button>("selling-companion-confirmation-no");

        sellingYes.RegisterOnSelected((evt) => viewDelegate.ConfirmSellCompanion());
        sellingNo.RegisterOnSelected((evt) => viewDelegate.StopSellingCompanion());

        originalSellingCompanionConfirmationText = rootVisualElement.Q<Label>("selling-companion-confirmation-label").text;
        originalSellingCompanionBreakdownText = rootVisualElement.Q<Label>("selling-companion-confirmation-breakdown-label").text;

        // Setup focusables
        FocusManager.Instance.RegisterFocusableTarget(sellingYes.AsFocusable());
        FocusManager.Instance.RegisterFocusableTarget(sellingNo.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(sellingYes.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(sellingNo.AsFocusable());
    }

    public void Hide() {
        visible = false;
        rootVisualElement.style.visibility = Visibility.Hidden;
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        // disable focusables
        FocusManager.Instance.DisableFocusableTarget(sellingYes.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(sellingNo.AsFocusable());
    }

    public void Show(CompanionManagementView companionView) {
        Debug.LogError("SellingCompanionConfirmationView Show");
        FocusManager.Instance.StashFocusables(this.GetType().Name);
        visible = true;
        rootVisualElement.style.visibility = Visibility.Visible;
        Label confirmSellCompanionLabel = rootVisualElement.Q<Label>("selling-companion-confirmation-label");
        CompanionSellValue sellValue = viewDelegate.CalculateCompanionSellPrice(companionView.companion);
        string replacedText = String.Format(
            originalSellingCompanionConfirmationText,
            companionView.companion.GetName(),
            sellValue.Total());
        confirmSellCompanionLabel.text = replacedText;
        Label sellCompanionBreakdownLabel = rootVisualElement.Q<Label>("selling-companion-confirmation-breakdown-label");
        string breakdownReplacedText = String.Format(
            originalSellingCompanionBreakdownText,
            sellValue.sellValueFromCompanions,
            sellValue.sellValueFromCardsBought,
            sellValue.sellValueFromCardsRemoved
        );
        sellCompanionBreakdownLabel.text = breakdownReplacedText;
        // Enable focusables
        FocusManager.Instance.EnableFocusableTarget(sellingYes.AsFocusable());
        FocusManager.Instance.EnableFocusableTarget(sellingNo.AsFocusable());
        FocusManager.Instance.SetFocusNextFrame(sellingNo.AsFocusable());
    }
}