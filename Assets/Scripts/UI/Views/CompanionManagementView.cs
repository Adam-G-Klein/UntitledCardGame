using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;

public class CompanionManagementView {
    public VisualElement container;
    public Companion companion;

    private ICompanionManagementViewDelegate viewDelegate;

    private VisualElement darkBox;

    public CompanionManagementView(Companion companion, ICompanionManagementViewDelegate viewDelegate) {
        this.viewDelegate = viewDelegate;
        container = MakeCompanionManagementView(companion);
        this.companion = companion;
    }

    public VisualElement MakeCompanionManagementView(Companion companion) {
        EntityView entityView = new EntityView(companion, 0, false);
        entityView.SetupEntityImage(companion.companionType.sprite);
        entityView.HideDescription();
        entityView.entityContainer.RegisterCallback<ClickEvent>(CompanionManagementOnClick);
        entityView.entityContainer.RegisterCallback<PointerDownEvent>(CompanionManagementOnPointerDown);
        entityView.entityContainer.RegisterCallback<PointerMoveEvent>(CompanionManagementOnPointerMove);
        entityView.entityContainer.RegisterCallback<PointerUpEvent>(ComapnionManagementOnPointerUp);
        entityView.entityContainer.RegisterCallback<PointerLeaveEvent>(ComapnionManagementOnPointerLeave);
        return entityView.entityContainer;
    }

    public void CompanionManagementOnClick(ClickEvent evt) {
        viewDelegate.CompanionManagementOnClick(this, evt);
    }

    private void CompanionManagementOnPointerDown(PointerDownEvent evt) {
        viewDelegate.CompanionManagementOnPointerDown(this, evt);
    }

    private void CompanionManagementOnPointerMove(PointerMoveEvent evt) {
        viewDelegate.CompanionManagementOnPointerMove(this, evt);
    }

    private void ComapnionManagementOnPointerUp(PointerUpEvent evt) {
        viewDelegate.ComapnionManagementOnPointerUp(this, evt);
    }

    private void ComapnionManagementOnPointerLeave(PointerLeaveEvent evt) {
        viewDelegate.CompanionManagementOnPointerLeave(this, evt);
    }

    public void ShowNotApplicable() {
        darkBox = new VisualElement();
        container.Add(darkBox);
        darkBox.style.position = Position.Absolute;
        darkBox.style.top = 0;
        darkBox.style.left = 0;
        darkBox.style.right= 0;
        darkBox.style.bottom = 0;
        darkBox.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
    }

    public void ResetApplicable() {
        if (darkBox != null) {
            darkBox.RemoveFromHierarchy();
            darkBox = null;
        }
    }
}