using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using FMODUnity;

public class CompanionManagementView : IControlsReceiver {
    public VisualElement container;
    public Companion companion;

    private ICompanionManagementViewDelegate viewDelegate;

    private VisualElement darkBox;
    private IconButton viewDeckButton = null;
    private IconButton sellCompanionButton = null;
    private VisualElement companionBoundingBox = null;
    private EntityView entityView;

    private bool draggingThisCompanion = false;

    public CompanionManagementView(Companion companion, ICompanionManagementViewDelegate viewDelegate) {
        this.viewDelegate = viewDelegate;
        container = MakeCompanionManagementView(companion);
        this.companion = companion;
    }

    public VisualElement MakeCompanionManagementView(Companion companion) {
        entityView = new EntityView(companion, 0, false, null, true);
        entityView.UpdateWidthAndHeight();

        entityView.entityContainer.RegisterCallback<ClickEvent>(CompanionManagementOnClick);

        entityView.entityContainer.RegisterCallback<PointerDownEvent>((evt) => CompanionManagementOnPointerDown(evt, true));
        entityView.entityContainer.RegisterCallback<PointerMoveEvent>(CompanionManagementOnPointerMove);
        entityView.entityContainer.RegisterCallback<PointerUpEvent>(ComapnionManagementOnPointerUp);

        entityView.entityContainer.RegisterCallback<PointerLeaveEvent>(ComapnionManagementOnPointerLeave);
        entityView.entityContainer.RegisterCallback<PointerEnterEvent>(CompanionManagementOnPointerEnter);

        entityView.entityContainer.name = companion.companionType.name;
        
        return entityView.entityContainer;
    }

    public void CompanionManagementOnPointerEnter(PointerEnterEvent evt)
    {
        if (viewDelegate.IsSellingCompanions() || viewDelegate.IsDraggingCompanion()) return;
        CreateViewDeckButton();
        CreateSellCompanionButton();
        CreateCompanionBoundingBox();
        viewDelegate.DisplayTooltip(entityView.entityContainer, companion.companionType.tooltip, true);
    }

    public void CompanionManagementNonMouseSelect() {
        if (!viewDelegate.CanDragCompanions()) {
            Debug.Log("Companion Management On Click not dragging");
            CompanionManagementOnClick(null);
        }
    }

    public void CompanionManagementOnClick(ClickEvent evt) {
        viewDelegate.CompanionManagementOnClick(this);
    }

    public void CompanionManagementOnPointerDown(PointerDownEvent evt, bool usingMouse) {
        Debug.Log("Companion on pointer down");
        RemoveCompanionHoverButtons();
        draggingThisCompanion = true;
        if (usingMouse) {
            viewDelegate.CompanionManagementOnPointerDown(this, evt.position);
        } else {
            // Handling dragging this companion using keyboard/controller
            FocusManager.Instance.onFocusDelegate += FocusChangedWhileDragging;
            viewDelegate.CompanionManagementOnPointerDown(this, (evt.target as VisualElement).worldBound.center);
            ControlsManager.Instance.RegisterControlsReceiver(this);
        }
    }

    public void FocusChangedWhileDragging(IFocusableTarget focusable) {
        viewDelegate.CompanionManagementOnPointerMove(this, focusable.GetUIPosition());
    }

    private void CompanionManagementOnPointerMove(PointerMoveEvent evt) {
        viewDelegate.CompanionManagementOnPointerMove(this, evt.position);
    }

    public void ComapnionManagementOnPointerUp(PointerUpEvent evt) {
        viewDelegate.ComapnionManagementOnPointerUp(this, evt.position);
        draggingThisCompanion = false;
    }

    public void ComapnionManagementOnPointerLeave(PointerLeaveEvent evt) {
        viewDelegate.CompanionManagementOnPointerLeave(this, evt);
        viewDelegate.DestroyTooltip(entityView.entityContainer);
    }

    public void CompanionManagementOnUnfocus() {
        viewDelegate.CompanionManagementOnPointerLeave(this, null);
        viewDelegate.DestroyTooltip(entityView.entityContainer);
        RemoveCompanionHoverButtons();
    }

    private void CreateViewDeckButton() {
        if (viewDeckButton != null) {
            viewDeckButton.RemoveFromHierarchy();
        } 
        viewDeckButton = new IconButton();
        viewDeckButton.AddToClassList("shopButton");
        viewDeckButton.AddToClassList("companion-view-deck-button");
        viewDeckButton.AddToClassList("icon-button-absolute");
        viewDeckButton.SetIconHeight(1f);
        viewDeckButton.text = "View Deck";
        viewDeckButton.SetIcon(GFGInputAction.VIEW_DECK, ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.VIEW_DECK));
        ControlsManager.Instance.RegisterIconChanger(viewDeckButton);

        viewDeckButton.style.width = container.worldBound.width;
        viewDeckButton.style.top = container.worldBound.yMin - container.worldBound.height * 0.3f;
        viewDeckButton.style.left = container.worldBound.xMin - 4;
        viewDeckButton.style.height = container.worldBound.height * 0.3f;
        viewDeckButton.name = "viewdeck";

        viewDeckButton.RegisterCallback<ClickEvent>((evt) => {ViewDeckButtonOnClick();});

        viewDelegate.AddToRoot(viewDeckButton);
    }

    private void CreateSellCompanionButton() {
        if (sellCompanionButton != null) {
            sellCompanionButton.RemoveFromHierarchy();
        }
        sellCompanionButton = new IconButton();
        sellCompanionButton.AddToClassList("shopButton");
        sellCompanionButton.AddToClassList("companion-sell-button");
        sellCompanionButton.AddToClassList("icon-button-absolute");
        sellCompanionButton.SetIconHeight(1f);
        sellCompanionButton.text = "Sell";
        sellCompanionButton.SetIcon(GFGInputAction.SELL_COMPANION, ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.SELL_COMPANION));
        ControlsManager.Instance.RegisterIconChanger(sellCompanionButton);

        sellCompanionButton.style.width = container.worldBound.width;
        sellCompanionButton.style.top = container.worldBound.yMax - 4;
        sellCompanionButton.style.left = container.worldBound.xMin - 4;
        sellCompanionButton.style.height = container.worldBound.height * 0.3f;
        sellCompanionButton.name = "sellcompanion";

        sellCompanionButton.RegisterCallback<ClickEvent>((evt) => { SellCompanionButtonOnClick(); });

        viewDelegate.AddToRoot(sellCompanionButton);
    }

    public void ViewDeckButtonOnClick() {
        RemoveCompanionHoverButtons();
        viewDelegate.ShowCompanionDeckView(companion);
    }

    public void SellCompanionButtonOnClick() {
        RemoveCompanionHoverButtons();
        viewDelegate.SellCompanion(this);
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

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (!draggingThisCompanion) return;

        if (action == GFGInputAction.SELECT_UP) {
            viewDelegate.ComapnionManagementOnPointerUp(this, FocusManager.Instance.GetCurrentFocus().GetUIPosition());
            FocusManager.Instance.onFocusDelegate -= FocusChangedWhileDragging;
            ControlsManager.Instance.UnregisterControlsReceiver(this);
            viewDelegate.DestroyTooltip(entityView.entityContainer);
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        // This is gonna be a whole ordeal
        return;
    }
}