using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CompanionManagementView {
    public VisualElement container;
    public Companion companion;

    private ICompanionManagementViewDelegate viewDelegate;

    private VisualElement darkBox;
    private Button viewDeckButton = null;
    private Button sellCompanionButton = null;
    private VisualElement companionBoundingBox = null;
    private EntityView entityView;

    public CompanionManagementView(Companion companion, ICompanionManagementViewDelegate viewDelegate) {
        this.viewDelegate = viewDelegate;
        container = MakeCompanionManagementView(companion);
        this.companion = companion;
    }

    public VisualElement MakeCompanionManagementView(Companion companion) {
        entityView = new EntityView(companion, 0, false, null, true);
        entityView.UpdateWidthAndHeight();
        //entityView.SetupEntityImage(companion.companionType.sprite);
        entityView.entityContainer.RegisterCallback<ClickEvent>(CompanionManagementOnClick);
        entityView.entityContainer.RegisterCallback<PointerDownEvent>(CompanionManagementOnPointerDown);
        entityView.entityContainer.RegisterCallback<PointerMoveEvent>(CompanionManagementOnPointerMove);
        entityView.entityContainer.RegisterCallback<PointerUpEvent>(ComapnionManagementOnPointerUp);
        entityView.entityContainer.RegisterCallback<PointerLeaveEvent>(ComapnionManagementOnPointerLeave);
        entityView.entityContainer.RegisterCallback<PointerEnterEvent>(CompanionManagementOnPointerEnter);
        entityView.entityContainer.name = companion.companionType.name;

        UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(entityView.entityContainer, 
            () => {CompanionManagementNonMouseSelect();}, 
            ()=> {CompanionManagementOnPointerEnter(null);},
            () => {ComapnionManagementOnPointerLeave(null);},
            HoverableType.CompanionManagement,
            companion.companionType);
        
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
        if(NonMouseInputManager.Instance.inputMethod == InputMethod.Mouse) {
            Debug.LogError("Got a callback for a non-mouse input method, but we're in mouse mode.");
            return;
        } 
        if(NonMouseInputManager.Instance.GetUIState() == UIState.DEFAULT) {
            CompanionManagementOnPointerDown(null);
        } else if(NonMouseInputManager.Instance.GetUIState() != UIState.DRAGGING_COMPANION) {
            CompanionManagementOnClick(null);
        } // if dragging, nonmouseinputmanager will callback with the pointer up event

    }

    public void CompanionManagementOnClick(ClickEvent evt) {
        viewDelegate.CompanionManagementOnClick(this, evt);
    }

    private void CompanionManagementOnPointerDown(PointerDownEvent evt) {
        RemoveCompanionHoverButtons();
        // stand-in for whatever logic we'll use to determine no mouse
        if(NonMouseInputManager.Instance.inputMethod != InputMethod.Mouse || evt == null) {
            NonMouseInputManager.Instance.CompanionDragACTIVATE(this, viewDelegate);
        } else {
            viewDelegate.CompanionManagementOnPointerDown(this, evt, NonMouseInputManager.Instance.currentlyHoveredScreenPosUiDoc());
        }
    }

    private void CompanionManagementOnPointerMove(PointerMoveEvent evt) {
        viewDelegate.CompanionManagementOnPointerMove(this, evt, NonMouseInputManager.Instance.currentlyHoveredScreenPosUiDoc());
    }

    private void ComapnionManagementOnPointerUp(PointerUpEvent evt) {
        viewDelegate.ComapnionManagementOnPointerUp(this, evt, NonMouseInputManager.Instance.currentlyHoveredScreenPosUiDoc());
        if(NonMouseInputManager.Instance.GetUIState() == UIState.DRAGGING_COMPANION) {
            NonMouseInputManager.Instance.SetUIState(UIState.DEFAULT);
        }
    }

    private void ComapnionManagementOnPointerLeave(PointerLeaveEvent evt) {
        viewDelegate.CompanionManagementOnPointerLeave(this, evt);
        viewDelegate.DestroyTooltip(entityView.entityContainer);
        // mouse removal / unhover is done w a bounding box.
        // we need to be more manual abt it w controllers
        /*if(NonMouseInputManager.Instance.currentlyHovered.associatedUIDocElement != viewDeckButton) {
            RemoveCompanionHoverButtons();
        }*/
        // massive fkn race condition sry folks
        if(NonMouseInputManager.Instance.inputMethod != InputMethod.Mouse) {
            List<VisualElement> elems = new List<VisualElement>
            {
                viewDeckButton,
                sellCompanionButton
            };
            UIDocumentHoverableInstantiator.Instance.CallIfNextHoverableNotInElemListWhenReady(
                RemoveCompanionHoverButtons, 
                elems);
        }
    }

    private void CreateViewDeckButton() {
        if (viewDeckButton != null) {
            UIDocumentHoverableInstantiator.Instance.CleanupHoverable(viewDeckButton);
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
        UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(viewDeckButton,
            () => {ViewDeckButtonOnClick();},
            ()=> {},
            ()=> {
                RemoveCompanionHoverButtonsIfNextHoverableNotInElemList();
            },
            HoverableType.DefaultShop);
    }

    private void RemoveCompanionHoverButtonsIfNextHoverableNotInElemList() {
        List<VisualElement> elems = new List<VisualElement>
        {
            viewDeckButton,
            sellCompanionButton,
            entityView.entityContainer
        };
        UIDocumentHoverableInstantiator.Instance.CallIfNextHoverableNotInElemListWhenReady(
            RemoveCompanionHoverButtons, 
            elems);
    }

    private void CreateSellCompanionButton() {
        if (sellCompanionButton != null) {
            sellCompanionButton.RemoveFromHierarchy();
            UIDocumentHoverableInstantiator.Instance.CleanupHoverable(sellCompanionButton);
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



        sellCompanionButton.RegisterCallback<ClickEvent>((evt) => {SellCompanionButtonOnClick();});

        viewDelegate.AddToRoot(sellCompanionButton);
        UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(sellCompanionButton,
            () => {SellCompanionButtonOnClick();},
            ()=> {},
            ()=> {
                RemoveCompanionHoverButtonsIfNextHoverableNotInElemList();
            },
            HoverableType.DefaultShop);
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
        // companionBoundingBox.AddToClassList("shopButton");
        viewDelegate.AddToRoot(companionBoundingBox);
        viewDelegate.GetMonoBehaviour().StartCoroutine(RegisterMoveCallback(companionBoundingBox.parent));
    }

    private IEnumerator RegisterMoveCallback(VisualElement parent) {
        yield return new WaitForEndOfFrame();
        parent.RegisterCallback<PointerMoveEvent>(BoundingBoxParentOnPointerMove);
    }


    private void BoundingBoxParentOnPointerMove(PointerMoveEvent evt) {
        if (!companionBoundingBox.worldBound.Contains(evt.position) && NonMouseInputManager.Instance.inputMethod == InputMethod.Mouse) {
            RemoveCompanionHoverButtons();
        }
    }

    private void RemoveCompanionHoverButtons() {
        if (sellCompanionButton != null) {
            sellCompanionButton.style.visibility = Visibility.Hidden;
            sellCompanionButton.RemoveFromHierarchy();
            UIDocumentHoverableInstantiator.Instance.CleanupHoverable(sellCompanionButton);
            sellCompanionButton = null;
        }
        if (viewDeckButton != null) {
            viewDeckButton.style.visibility = Visibility.Hidden; 
            viewDeckButton.RemoveFromHierarchy();
            UIDocumentHoverableInstantiator.Instance.CleanupHoverable(viewDeckButton);
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