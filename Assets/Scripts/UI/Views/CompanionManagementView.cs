using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using Unity.VisualScripting;

public class CompanionManagementView {
    public VisualElement container;
    public Companion companion;

    private ICompanionManagementViewDelegate viewDelegate;

    private VisualElement darkBox;
    private Button viewDeckButton = null;
    private Button sellCompanionButton = null;
    private VisualElement companionBoundingBox = null;

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
        entityView.entityContainer.RegisterCallback<PointerEnterEvent>(CompanionManagementOnPointerEnter);
        return entityView.entityContainer;
    }

    private void CompanionManagementOnPointerEnter(PointerEnterEvent evt)
    {
        if (viewDelegate.IsSellingCompanions() || viewDelegate.IsDraggingCompanion()) return;
        CreateViewDeckButton();
        CreateSellCompanionButton();
        CreateCompanionBoundingBox();
    }

    public void CompanionManagementOnClick(ClickEvent evt) {
        viewDelegate.CompanionManagementOnClick(this, evt);
    }

    private void CompanionManagementOnPointerDown(PointerDownEvent evt) {
        RemoveCompanionHoverButtons();
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

    private void CreateViewDeckButton() {
        if (viewDeckButton != null) viewDeckButton.RemoveFromHierarchy();
        viewDeckButton = new Button();
        viewDeckButton.AddToClassList("shopButton");
        viewDeckButton.AddToClassList("companion-view-deck-button");
        viewDeckButton.text = "View Deck";

        viewDeckButton.style.width = container.worldBound.width;
        viewDeckButton.style.top = container.worldBound.yMin - container.worldBound.height * 0.3f;
        viewDeckButton.style.left = container.worldBound.xMin - 4;
        viewDeckButton.style.height = container.worldBound.height * 0.3f;

        viewDeckButton.clicked += ViewDeckButtonOnClick;

        viewDelegate.AddToRoot(viewDeckButton);
    }

    private void CreateSellCompanionButton() {
        Debug.LogError("CreateSellCompanionButton");
        if (sellCompanionButton != null) sellCompanionButton.RemoveFromHierarchy();
        sellCompanionButton = new Button();
        sellCompanionButton.AddToClassList("shopButton");
        sellCompanionButton.AddToClassList("companion-sell-button");
        sellCompanionButton.text = "Sell";

        sellCompanionButton.style.width = container.worldBound.width;
        sellCompanionButton.style.top = container.worldBound.yMax - 4;
        sellCompanionButton.style.left = container.worldBound.xMin - 4;
        sellCompanionButton.style.height = container.worldBound.height * 0.3f;

        sellCompanionButton.clicked += SellCompanionButtonOnClick;

        viewDelegate.AddToRoot(sellCompanionButton);
    }

    private void ViewDeckButtonOnClick() {
        RemoveCompanionHoverButtons();
        viewDelegate.ShowCompanionDeckView(companion);
    }

    private void SellCompanionButtonOnClick() {
        RemoveCompanionHoverButtons();
        viewDelegate.SellCompanion(companion);
    }

    private void CreateCompanionBoundingBox() {
        companionBoundingBox = new VisualElement();
        companionBoundingBox.style.position = Position.Absolute;
        companionBoundingBox.pickingMode = PickingMode.Ignore;
        float width = container.worldBound.width * 1.5f;
        float height = container.worldBound.height * 1.5f;
        companionBoundingBox.style.width = width;
        companionBoundingBox.style.height = height;
        Vector2 containerCenter = new Vector2(container.worldBound.x + container.worldBound.width * 0.5f,
            container.worldBound.y + container.worldBound.height * 0.5f);
        companionBoundingBox.style.left = containerCenter.x - (width * 0.5f);
        companionBoundingBox.style.top = containerCenter.y - (height * 0.5f);
        // companionBoundingBox.AddToClassList("shopButton");
        viewDelegate.AddToRoot(companionBoundingBox);
        viewDelegate.GetMonoBehaviour().StartCoroutine(RegisterMoveCallback(companionBoundingBox.parent));
    }

    private IEnumerator RegisterMoveCallback(VisualElement parent) {
        yield return new WaitForEndOfFrame();
        parent.RegisterCallback<PointerMoveEvent>(BoundingBoxParentOnPointerMove);
    }


    private void BoundingBoxParentOnPointerMove(PointerMoveEvent evt) {
        if (!companionBoundingBox.worldBound.Contains(evt.position)) {
            RemoveCompanionHoverButtons();
        }
    }

    private void RemoveCompanionHoverButtons() {
        if (sellCompanionButton != null) {
            sellCompanionButton.style.visibility = Visibility.Hidden;
            sellCompanionButton.RemoveFromHierarchy();
            sellCompanionButton = null;
        }
        if (viewDeckButton != null) {
            viewDeckButton.style.visibility = Visibility.Hidden; 
            viewDeckButton.RemoveFromHierarchy();
            viewDeckButton = null;
        }
        if (companionBoundingBox != null) {
            companionBoundingBox.parent.UnregisterCallback<PointerMoveEvent>(BoundingBoxParentOnPointerMove);
            companionBoundingBox.RemoveFromHierarchy();
            companionBoundingBox = null;
        }
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