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
    public VisualElementFocusable visualElementFocusable;

    private ICompanionManagementViewDelegate viewDelegate;

    private VisualElement darkBox;
    private Button viewDeckButton = null;
    private Button sellCompanionButton = null;
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
        visualElementFocusable = entityView.elementFocusable;
        entityView.UpdateWidthAndHeight();

        entityView.entityContainer.RegisterCallback<ClickEvent>(CompanionManagementOnClick);
        entityView.entityContainer.RegisterCallback<NavigationSubmitEvent>((evt) => CompanionManagementNonMouseSelect());

        entityView.entityContainer.RegisterCallback<PointerDownEvent>(CompanionManagementOnPointerDown);
        visualElementFocusable.SetInputAction(GFGInputAction.SELECT_DOWN, () => CompanionManagementOnPointerDown(null));
        entityView.entityContainer.RegisterCallback<PointerMoveEvent>(CompanionManagementOnPointerMove);
        entityView.entityContainer.RegisterCallback<PointerUpEvent>(ComapnionManagementOnPointerUp);

        entityView.entityContainer.RegisterCallback<PointerLeaveEvent>(ComapnionManagementOnPointerLeave);
        entityView.entityContainer.RegisterCallback<PointerEnterEvent>(CompanionManagementOnPointerEnter);
        visualElementFocusable.additionalFocusAction += () => CompanionManagementOnPointerEnter(null);
        visualElementFocusable.additionalUnfocusAction += CompanionManagementOnUnfocus;

        entityView.entityContainer.name = companion.companionType.name;

        FocusManager.Instance.RegisterFocusableTarget(visualElementFocusable);
        
        return entityView.entityContainer;
    }

    private void CompanionManagementOnPointerEnter(PointerEnterEvent evt)
    {
        if (viewDelegate.IsSellingCompanions() || viewDelegate.IsDraggingCompanion()) return;
        CreateViewDeckButton();
        CreateSellCompanionButton();
        CreateCompanionBoundingBox();
        viewDelegate.DisplayTooltip(entityView.entityContainer, companion.companionType.tooltip, true);
    }

    public void CompanionManagementNonMouseSelect() {
        if (!viewDelegate.CanDragCompanions()) {
            CompanionManagementOnClick(null);
        }
    }

    public void CompanionManagementOnClick(ClickEvent evt) {
        viewDelegate.CompanionManagementOnClick(this);
    }

    private void CompanionManagementOnPointerDown(PointerDownEvent evt) {
        RemoveCompanionHoverButtons();
        draggingThisCompanion = true;
        if (evt == null) {
            // Handling dragging this companion using keyboard/controller
            FocusManager.Instance.onFocusDelegate += FocusChangedWhileDragging;
            Vector2 currentPos = visualElementFocusable.GetUIPosition();
            FocusManager.Instance.DisableFocusableTarget(visualElementFocusable);
            viewDelegate.CompanionManagementOnPointerDown(this, currentPos);
            ControlsManager.Instance.RegisterControlsReceiver(this);
        } else {
            viewDelegate.CompanionManagementOnPointerDown(this, evt.position);
        }
    }

    private void FocusChangedWhileDragging(IFocusableTarget focusable) {
        Debug.Log("poopopp " + focusable.GetUIPosition());
        viewDelegate.CompanionManagementOnPointerMove(this, focusable.GetUIPosition());
    }

    private void CompanionManagementOnPointerMove(PointerMoveEvent evt) {
        viewDelegate.CompanionManagementOnPointerMove(this, evt.position);
    }

    private void ComapnionManagementOnPointerUp(PointerUpEvent evt) {
        viewDelegate.ComapnionManagementOnPointerUp(this, evt.position);
        draggingThisCompanion = false;
    }

    private void ComapnionManagementOnPointerLeave(PointerLeaveEvent evt) {
        viewDelegate.CompanionManagementOnPointerLeave(this, evt);
        viewDelegate.DestroyTooltip(entityView.entityContainer);
    }

    private void CompanionManagementOnUnfocus() {
        viewDelegate.CompanionManagementOnPointerLeave(this, null);
        viewDelegate.DestroyTooltip(entityView.entityContainer);
        RemoveCompanionHoverButtons();
    }

    private void CreateViewDeckButton() {
        if (viewDeckButton != null) {
            viewDeckButton.RemoveFromHierarchy();
        } 
        viewDeckButton = new Button();
        viewDeckButton.AddToClassList("shopButton");
        viewDeckButton.AddToClassList("companion-view-deck-button");
        viewDeckButton.text = "View Deck";

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
        sellCompanionButton = new Button();
        sellCompanionButton.AddToClassList("shopButton");
        sellCompanionButton.AddToClassList("companion-sell-button");
        sellCompanionButton.text = "Sell";

        sellCompanionButton.style.width = container.worldBound.width;
        sellCompanionButton.style.top = container.worldBound.yMax - 4;
        sellCompanionButton.style.left = container.worldBound.xMin - 4;
        sellCompanionButton.style.height = container.worldBound.height * 0.3f;
        sellCompanionButton.name = "sellcompanion";



        sellCompanionButton.RegisterCallback<ClickEvent>((evt) => { SellCompanionButtonOnClick(); });

        viewDelegate.AddToRoot(sellCompanionButton);
    }

    private void ViewDeckButtonOnClick() {
        RemoveCompanionHoverButtons();
        viewDelegate.ShowCompanionDeckView(companion);
    }

    private void SellCompanionButtonOnClick() {
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
            FocusManager.Instance.EnableFocusableTarget(visualElementFocusable);
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        // This is gonna be a whole ordeal
        throw new NotImplementedException();
    }
}