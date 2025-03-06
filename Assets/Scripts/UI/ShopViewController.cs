using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ShopViewController : MonoBehaviour, 
    IShopItemViewDelegate,
    ICompanionManagementViewDelegate
    
{
    public UIDocument uiDoc;
    public bool canDragCompanions = false;
    public Color slotHighlightColor;
    public Color slotNotHighlightColor;
    public Color slotUnavailableColor;
    public GameObject cardSelectionViewPrefab;

    private ShopManager shopManager;
    private Dictionary<CardInShopWithPrice, ShopItemView> cardItemToViewMap;
    private Dictionary<CompanionInShopWithPrice, ShopItemView> companionItemToViewMap;

    // Specific shop VisualElement references
    private VisualElement shopGoodsArea;
    private ScrollView benchScrollView;
    private VisualElement activeContainer;
    private VisualElement mapContainer;
    private Button upgradeButton;
    private Button rerollButton;
    private Label moneyLabel;
    private Label notEnoughMoneyLabel;
    public VisualElement selectingCompanionVeil;
    public VisualElement selectingIndicator;
    public Button selectingCancelButton;
    public Label upgradePriceLabel;
    public Label rerollPriceLabel;
    //public Button sellCompanionButton;
    public VisualElement sellingCompanionConfirmation;
    private VisualElement deckView;
    private VisualElement deckViewContentContainer;
    private VisualElement genericMessageBox;

    // For dragging and dropping companions in the unit management
    private bool isDraggingCompanion = false;
    private VisualElement companionBeingDragged = null;
    private VisualElement originalParent = null;
    private Dictionary<VisualElement, CompanionManagementView> visualElementToCompanionViewMap = new Dictionary<VisualElement, CompanionManagementView>();
    public List<VisualElement> blockedSlots = new List<VisualElement>();

    private IEnumerator notEnoughMoneyCoroutine;
    private IEnumerator upgradeButtonTooltipCoroutine = null;
    private VisualElement tooltip;
    private bool sellingCompanions = false;
    private CompanionManagementView companionToSell;
    private string originalSellingCompanionConfirmationText;
    private bool inUpgradeMenu = false;
    private Dictionary<VisualElement, GameObject> tooltipMap = new();

    public void Start() {
        // Init(null);
    }

    public void Init(ShopManager shopManager) {
        if (uiDoc == null) {
            uiDoc = GetComponent<UIDocument>();
        }

        this.shopManager = shopManager;

        cardItemToViewMap = new Dictionary<CardInShopWithPrice, ShopItemView>();
        companionItemToViewMap = new Dictionary<CompanionInShopWithPrice, ShopItemView>();

        mapContainer = uiDoc.rootVisualElement.Q("map");
        SetupMap(shopManager);
        shopGoodsArea = uiDoc.rootVisualElement.Q("shop-goods-area");
        activeContainer = uiDoc.rootVisualElement.Q("unit-active-container");
        benchScrollView = uiDoc.rootVisualElement.Q<ScrollView>("bench-scroll-view");
        moneyLabel = uiDoc.rootVisualElement.Q<Label>("money-indicator-label");
        notEnoughMoneyLabel = uiDoc.rootVisualElement.Q<Label>("not-enough-money-indicator");
        selectingCompanionVeil = uiDoc.rootVisualElement.Q("selecting-companion-veil");
        selectingIndicator = uiDoc.rootVisualElement.Q("companion-selection-indicator");
        selectingCancelButton = uiDoc.rootVisualElement.Q<Button>("companion-selection-cancel-button");
        upgradePriceLabel = uiDoc.rootVisualElement.Q<Label>("upgrade-price-label");
        rerollPriceLabel = uiDoc.rootVisualElement.Q<Label>("reroll-price-label");
        //sellCompanionButton = uiDoc.rootVisualElement.Q<Button>("sell-companion-button");
        sellingCompanionConfirmation = uiDoc.rootVisualElement.Q("selling-companion-confirmation");
        deckView = uiDoc.rootVisualElement.Q("deck-view");
        deckViewContentContainer = uiDoc.rootVisualElement.Q("deck-view-card-area");
        genericMessageBox = uiDoc.rootVisualElement.Q("generic-message-box");

        SetupActiveSlots(shopManager.gameState.companions.currentCompanionSlots);

        rerollButton = uiDoc.rootVisualElement.Q<Button>("reroll-button");
        rerollButton.RegisterCallback<ClickEvent>(RerollButtonOnClick);

        UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(rerollButton,
                () => {RerollButtonOnClick(null);}, 
                () => {
                    // hi Ethan this is where a reroll onEnterCallBack would go to hook it up to the hoverable system :)
                }, 
                () => {
                    // hi Ethan this is where a reroll onLeaveCallBack would go to hook it up to the hoverable system :)
                });
        selectingCancelButton.RegisterCallback<ClickEvent>(CancelCardBuy);
        upgradeButton = uiDoc.rootVisualElement.Q<Button>("upgrade-button");
        upgradeButton.RegisterCallback<ClickEvent>(UpgradeButtonOnClick);
        upgradeButton.RegisterCallback<PointerEnterEvent>(UpgradeButtonOnPointerEnter);
        UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(upgradeButton,
                () => {UpgradeButtonOnClick(null);}, 
                () => {UpgradeButtonOnPointerEnter(null);}, 
                () => {UpgradeButtonOnPointerLeave(null);});
        upgradeButton.RegisterCallback<PointerLeaveEvent>(UpgradeButtonOnPointerLeave);
        uiDoc.rootVisualElement.Q<Button>("start-next-combat-button").RegisterCallback<ClickEvent>(StartNextCombatOnClick);
        UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(uiDoc.rootVisualElement.Q<Button>("start-next-combat-button"),
                () => {StartNextCombatOnClick(null);}, 
                () => {},
                () => {});
        //sellCompanionButton.clicked += SellCompanionOnClick;
        sellingCompanionConfirmation.Q<Button>("selling-companion-confirmation-yes").clicked += ConfirmSellCompanion;
        sellingCompanionConfirmation.Q<Button>("selling-companion-confirmation-no").clicked += StopSellingCompanion;
        originalSellingCompanionConfirmationText = sellingCompanionConfirmation.Q<Label>("selling-companion-confirmation-label").text;
        deckView.Q<Button>().clicked += CloseCompanionDeckView;

        //setup upgradeMenu
        uiDoc.rootVisualElement.Q<Button>(name:"cancelUpgrade").clicked += CancelUpgrade;
        uiDoc.rootVisualElement.Q<Button>(name:"confirmUpgrade").clicked += ConfirmUpgrade;
    }

    private void SetupActiveSlots(int numCompanions) {
        if (numCompanions >= 5) return;

        blockedSlots = new List<VisualElement>();
        int i = 0;
        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            i++;
            if (i > numCompanions) {
                child.style.backgroundColor = slotUnavailableColor;
                blockedSlots.Add(child);
            }
        }
    }

    private void SetupMap(IEncounterBuilder encounterBuilder) {
        mapContainer.Clear();
        Label mapTitle = new Label();
        mapTitle.AddToClassList("map-title");
        mapTitle.text = "Map";
        mapContainer.Add(mapTitle);
        Label combatCounter = new Label();
        combatCounter.AddToClassList("map-combat-counter");
        int totalCombats = (ShopManager.Instance.gameState.map.GetValue().encounters.Count + 1) / 2;
        int wonCombats = (ShopManager.Instance.gameState.currentEncounterIndex / 2) + 1;
        combatCounter.text = "Combats Won: " + wonCombats + "/" + totalCombats;
        mapContainer.Add(combatCounter);
        mapContainer.Add(new MapView(encounterBuilder).mapContainer);
    }

    public void Clear() {
        ClearUnitManagement();
        shopGoodsArea.Clear();
        SetupActiveSlots(shopManager.gameState.companions.currentCompanionSlots);
    }

    public void AddCardToShopView(CardInShopWithPrice card) {
        ShopItemView newCardItemView = new ShopItemView(this, card);

        shopGoodsArea.Add(newCardItemView.shopItemElement);

        cardItemToViewMap.Add(card, newCardItemView);
    }

    public void RemoveCardFromShopView(CardInShopWithPrice card) {
        ShopItemView shopItemView = cardItemToViewMap[card];

        // TODO: Replace with sold out? Grey it out? Talk to Jasmine
        shopItemView.Disable();
        //shopGoodsArea.Remove(shopItemView.shopItemElement);
        UIDocumentHoverableInstantiator.Instance.CleanupHoverable(shopItemView.shopItemElement);
        
        cardItemToViewMap.Remove(card);
    }

    public void AddCompanionToShopView(CompanionInShopWithPrice companion) {
        ShopItemView newCompanionItemView = new ShopItemView(this, companion);

        shopGoodsArea.Add(newCompanionItemView.shopItemElement);

        companionItemToViewMap.Add(companion, newCompanionItemView);
    }

    public void RemoveCompanionFromShopView(CompanionInShopWithPrice companion) {
        ShopItemView shopItemView = companionItemToViewMap[companion];

        // TODO: Replace with sold out? Grey it out? Talk to Jasmine
        //shopGoodsArea.Remove(shopItemView.shopItemElement);
        shopItemView.Disable();
        UIDocumentHoverableInstantiator.Instance.CleanupHoverable(shopItemView.shopItemElement);
        companionItemToViewMap.Remove(companion);
    }

    public void RebuildUnitManagement(CompanionListVariableSO companionList) {
        ClearUnitManagement();
        SetupActiveSlots(shopManager.gameState.companions.currentCompanionSlots);
        SetupUnitManagement(companionList);
    }

    private void ClearUnitManagement() {
        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            if (child.childCount > 0) {
                foreach(VisualElement grandchild in child.Children()) {
                    UIDocumentHoverableInstantiator.Instance.CleanupHoverable(grandchild);
                }
                child.Clear();
            }
            child.style.backgroundColor = slotNotHighlightColor;
        }
        foreach (VisualElement child in benchScrollView.contentContainer.hierarchy.Children()) {
            if (child.childCount > 0) {
                foreach(VisualElement grandchild in child.Children()) {
                    UIDocumentHoverableInstantiator.Instance.CleanupHoverable(grandchild);
                }
                child.Clear();
            }
            child.style.backgroundColor = slotNotHighlightColor;
        }
        blockedSlots = new List<VisualElement>();
    }

    public void SetupUnitManagement(CompanionListVariableSO companionList) {
        SetupActiveCompanions(companionList.activeCompanions);
        SetupBenchCompanions(companionList.benchedCompanions);
    }

    public void SetupActiveCompanions(List<Companion> companions) {
        for (int i = 0; i < companions.Count; i++) {
            CompanionManagementView companionView = new CompanionManagementView(companions[i], this);
            activeContainer.contentContainer[i].Add(companionView.container);
            visualElementToCompanionViewMap.Add(companionView.container, companionView);
        }
    }

    public void SetupBenchCompanions(List<Companion> companions) {
        float fixedWidth = activeContainer.contentContainer[0].resolvedStyle.width;
        // By default, the UI contains slots for 5 bench companions
        // If we have more than 5, we'll need to programatically add more
        // The +1 makes it so we always have one open slot, in case the player wants
        // to JUST move a companion from active to bench, and not swap one for another
        /*int slotsToAdd = companions.Count - 5 + 1;
        if (slotsToAdd > 0) {
            for (int i = 0; i < slotsToAdd; i++) {
                CreateNewBenchSlot();
            }
        }*/
        for (int i = 0; i < companions.Count; i++) {
            CompanionManagementView companionView = new CompanionManagementView(companions[i], this);
            benchScrollView.contentContainer[i].Add(companionView.container);
            visualElementToCompanionViewMap.Add(companionView.container, companionView);
        }

        // I can't fully figure out why this is necessary. Since we're using a scroll view,
        // as more items are added, if we use a fixed percent width for the items, then as the 
        // content visual element gets bigger, the items inside update to be a certain percent
        // of the new larger size. This sets them to the size they originally have in the base
        // UI document on scene start.
        for (int i = 0; i < benchScrollView.contentContainer.childCount; i++) {
            benchScrollView.contentContainer[i].style.width = new StyleLength(fixedWidth);
        }
    }

    private void CreateNewBenchSlot() {
        VisualElement newSlot = new VisualElement();
        newSlot.AddToClassList("single-unit-container");
        benchScrollView.Add(newSlot);

        float fixedWidth = benchScrollView.contentContainer[0].resolvedStyle.width;
        newSlot.style.width = new StyleLength(fixedWidth);
    }

    public void ShopItemOnClick(ShopItemView shopItemView) {
        if (shopItemView.companionInShop != null) {
            shopManager.ProcessCompanionBuyRequest(shopItemView, shopItemView.companionInShop);
        } else if (shopItemView.cardInShop != null) {
            shopManager.ProcessCardBuyRequestV2(shopItemView, shopItemView.cardInShop);
        }
    }

    public void RerollButtonOnClick(ClickEvent evt) {
        shopManager.ProcessRerollShopClick();
    }

    public void UpgradeButtonOnClick(ClickEvent evt) {
        shopManager.ProcessUpgradeShopClick();
    }

    public void StartNextCombatOnClick(ClickEvent evt) {
        shopManager.exitShop();
    }

    public void SellCompanionOnClick() {
        if (sellingCompanions) {
            //sellCompanionButton.text = "Sell Companion";
            StopSellingCompanion();
        } else {
            //sellCompanionButton.text = "Cancel";
            SetupSellCompanion();
        }
    }

    public void SetupSellCompanion() {
        sellingCompanions = true;
        selectingCompanionVeil.style.visibility = Visibility.Visible;
        canDragCompanions = false;
    }

    public void StopSellingCompanion() {
        sellingCompanions = false;
        selectingCompanionVeil.style.visibility = Visibility.Hidden;
        canDragCompanions = true;
        companionToSell = null;
        sellingCompanionConfirmation.style.visibility = Visibility.Hidden;
    }

    public void CompanionManagementOnClick(CompanionManagementView companionView, ClickEvent evt)
    {
        if (sellingCompanions) {
            sellingCompanionConfirmation.style.visibility = Visibility.Visible;
            Label confirmSellCompanionLabel = sellingCompanionConfirmation.Q<Label>("selling-companion-confirmation-label");
            string replacedText = String.Format(
                originalSellingCompanionConfirmationText, 
                companionView.companion.GetName(), 
                shopManager.CalculateCompanionSellPrice(companionView.companion));
            confirmSellCompanionLabel.text = replacedText;
        } else {
            shopManager.ProcessCompanionClicked(companionView.companion);
        }
    }

    private void ConfirmSellCompanion() {
        shopManager.SellCompanion(companionToSell.companion, companionToSell.container);
        companionToSell = null;
        sellingCompanions = false;
        canDragCompanions = true;
        selectingCompanionVeil.style.visibility = Visibility.Hidden;
        sellingCompanionConfirmation.style.visibility = Visibility.Hidden;
    }

    private void DontSellCompanion() {
        companionToSell = null;
        sellingCompanionConfirmation.style.visibility = Visibility.Hidden;
    }

    public void CompanionManagementOnPointerDown(CompanionManagementView companionView, PointerDownEvent evt, Vector2 pointerScreenPos)
    {
        if (!canDragCompanions && !isDraggingCompanion) return;

        Debug.Log("[ControllerDrag] evt position: " + (evt == null ? "evt is null" : evt.position) + " pointer screen pos: " + pointerScreenPos);

        Vector2 pos = evt == null ? pointerScreenPos : evt.position;

        VisualElement parent = companionView.container.parent;

        VisualElement tempContainer = new VisualElement();
        tempContainer.style.width = parent.resolvedStyle.width;
        tempContainer.style.height = parent.resolvedStyle.height;
        tempContainer.style.position = Position.Absolute;

        uiDoc.rootVisualElement.Add(tempContainer);
        StartCoroutine(FinishDragSetup(tempContainer, companionView.container, pos, parent));
    }

    private IEnumerator FinishDragSetup(VisualElement tempContainer, VisualElement companion, Vector2 position, VisualElement originalSpot) {
        yield return new WaitForEndOfFrame();
        tempContainer.style.top = position.y - tempContainer.layout.height / 2;
        tempContainer.style.left = position.x - tempContainer.layout.width / 2;
        originalParent = originalSpot;
        originalSpot.Remove(companion);
        originalSpot.style.backgroundColor = slotHighlightColor;
        tempContainer.Add(companion);
        isDraggingCompanion = true;
        companionBeingDragged = companion;
    }

    // TODO: replace evt with passed-in equivalent position
    public void CompanionManagementOnPointerMove(CompanionManagementView companionManagementView, PointerMoveEvent evt, Vector2 pointerScreenPos)
    {        
        if (!isDraggingCompanion || companionBeingDragged != companionManagementView.container) {
            if(evt == null) {
                Debug.LogError("OnPointerMove Called from not mouse But Not dragging companion or not the companion being dragged");
            }
            return;
        }  

        Debug.Log("[ControllerDrag] evt position: " + (evt == null ? "evt is null" : evt.position) + " pointer screen pos: " + pointerScreenPos);
        Vector2 pos = evt == null ? pointerScreenPos : evt.position;
        companionManagementView.container.parent.style.top = pos.y - companionManagementView.container.parent.layout.height / 2;
        companionManagementView.container.parent.style.left = pos.x - companionManagementView.container.parent.layout.width / 2;

        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            if (blockedSlots != null && blockedSlots.Contains(child)) continue;
            if (child.worldBound.Contains(pos)) {
                child.style.backgroundColor = slotHighlightColor;
                if (NumOpenSlots(activeContainer.Children().ToList(), true) < 5 - blockedSlots.Count) {
                    MoveWhileDragging(activeContainer, child);
                    originalParent = child;
                }
            } else {
                child.style.backgroundColor = slotNotHighlightColor;
            }
        }
        foreach (VisualElement child in benchScrollView.contentContainer.hierarchy.Children()) {
            if (child.worldBound.Contains(pos)) {
                if (NumOpenSlots(benchScrollView.contentContainer.Children().ToList(), true) < 5) {
                    MoveWhileDragging(benchScrollView.contentContainer, child);
                    originalParent = child;
                }
                child.style.backgroundColor = slotHighlightColor;
            } else {
                child.style.backgroundColor = slotNotHighlightColor;
            }
        }
    }

    private void MoveWhileDragging(VisualElement parentContainer,VisualElement elementOver) {
        List<VisualElement> elements = parentContainer.Children().ToList();
        // pull all of the existing companions into a list and then iterate over the available slots in activecontainer. skip over the element over
        List<VisualElement> companions = new List<VisualElement>();
        foreach (VisualElement unitContainer in elements) {
            if (unitContainer.childCount == 1) {
                companions.Add(unitContainer[0]);
            }
        }

        if (companions.Count() == 0) {
            if (parentContainer == activeContainer) {
                RefreshContainers(benchScrollView.contentContainer.Children().ToList(), true);
            } else {
                RefreshContainers(activeContainer.Children().ToList(), false);
            }
            return;
        }
        int companionIndex = 0;
        for (int i = 0; i < parentContainer.Children().Count(); i++) {
            VisualElement child = parentContainer.Children().ElementAt(i);
            if (child == elementOver) continue;
            child.Clear();
            child.Add(companions[companionIndex++]);
            if (i == parentContainer.Children().Count() || companionIndex == companions.Count) {
                if (parentContainer == activeContainer) {
                    RefreshContainers(benchScrollView.contentContainer.Children().ToList(), true);
                } else {
                    RefreshContainers(activeContainer.Children().ToList(), false);
                }
                break;
            }
        }
    }

    public void CompanionManagementOnPointerLeave(CompanionManagementView companionManagementView, PointerLeaveEvent evt) {
        if (!isDraggingCompanion || companionBeingDragged != companionManagementView.container)  return;

        companionManagementView.container.parent.style.top = evt.position.y - companionManagementView.container.parent.layout.height / 2;
        companionManagementView.container.parent.style.left = evt.position.x - companionManagementView.container.parent.layout.width / 2;
    }

    public void ComapnionManagementOnPointerUp(CompanionManagementView companionManagementView, PointerUpEvent evt)
    {
        if (!isDraggingCompanion || companionManagementView.container != companionBeingDragged) return;
        
        VisualElement elementOver = null;
        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            if (child.worldBound.Contains(evt.position)) {
                elementOver = child;
            }
        }
        foreach (VisualElement child in benchScrollView.contentContainer.hierarchy.Children()) {
            if (child.worldBound.Contains(evt.position)) {
                elementOver = child;
            }
        }

        if (elementOver != null && !blockedSlots.Contains(elementOver)) {
            DoMoveCompanion(companionManagementView, elementOver);
            elementOver.style.backgroundColor = slotNotHighlightColor;
        } else {
            VisualElement tempContainer = companionManagementView.container.parent;
            originalParent.Add(companionManagementView.container);
            uiDoc.rootVisualElement.Remove(tempContainer);
        }
        isDraggingCompanion = false;
        companionBeingDragged = null;
        originalParent = null;
    }

    private void DoMoveCompanion(CompanionManagementView companionManagementView, VisualElement movingToContainer) {
        // Scenario 1, dragging companion to open container
        if (movingToContainer.childCount == 0) {
            VisualElement tempContainer = companionManagementView.container.parent;

            // SPECIAL CASE ALERT: SPECIAL CASE A L E R T
            // Trying to move last active companion to *possibly* the bench
            if (!shopManager.CanMoveCompanionToNewOpenSlot(companionManagementView.companion)) {
                originalParent.Add(companionManagementView.container);
                uiDoc.rootVisualElement.Remove(tempContainer);
            } else {
                movingToContainer.Add(companionManagementView.container);
                tempContainer.RemoveFromHierarchy();
            }
        // Scenario 2, dragging companion to slot with another companion in it already
        } else if (movingToContainer.childCount == 1) {
            originalParent.Add(movingToContainer[0]);
            VisualElement tempContainer = companionManagementView.container.parent;
            movingToContainer.Add(companionManagementView.container);
            tempContainer.RemoveFromHierarchy();
        } else {
            Debug.LogError("Companion container contains more than 1 element in heirarchy");
        }
        RefreshContainers(activeContainer.Children().ToList(), false);
        RefreshContainers(benchScrollView.contentContainer.Children().ToList(), true);
        SetCompanionOrdering();
    }

    private int NumOpenSlots(List<VisualElement> unitContainers, bool isBench) {
        int takenSlots = 0;
        if (!isBench) takenSlots += blockedSlots.Count();
        foreach (VisualElement unitContainer in unitContainers) {
            if (unitContainer.childCount == 1) {
                takenSlots += 1;
            }
        }
        return takenSlots;
    }

    private void SetCompanionOrdering() {
        List<Companion> activeCompanions = new List<Companion>();
        activeContainer.Children().ToList().ForEach((ve) => {
            if (ve.childCount == 1) activeCompanions.Add(visualElementToCompanionViewMap[ve[0]].companion);
        });
        List<Companion> benchCompanions = new List<Companion>();
        benchScrollView.contentContainer.Children().ToList().ForEach((ve) => {
            if (ve.childCount == 1) benchCompanions.Add(visualElementToCompanionViewMap[ve[0]].companion);
        });
        shopManager.SetCompanionOrdering(activeCompanions, benchCompanions);
    }

    private void RefreshContainers(List<VisualElement> unitContainers, bool isBench) {
        List<VisualElement> companions = new List<VisualElement>();
        foreach (VisualElement unitContainer in unitContainers) {
            if (unitContainer.childCount == 1) {
                companions.Add(unitContainer[0]);
            }
        }

        for (int i = 0; i < companions.Count; i++) {
            unitContainers[i].Add(companions[i]);
        }

        /*if (companions.Count == unitContainers.Count && isBench) {
            // This makes it so that the player can always move a companion to the bench without
            // needing to swap one companion for another
            CreateNewBenchSlot();
        }*/
    }

    public void SetMoney(int money) {
        moneyLabel.text = money.ToString();
    }

    public void NotEnoughMoney() {
        if (notEnoughMoneyCoroutine == null) {
            notEnoughMoneyCoroutine = ShowNotEnoughMoney();
            StartCoroutine(notEnoughMoneyCoroutine);
        }
    }

    private IEnumerator ShowNotEnoughMoney() {
        notEnoughMoneyLabel.style.visibility = Visibility.Visible;
        yield return new WaitForSeconds(3f);
        notEnoughMoneyLabel.style.visibility = Visibility.Hidden;
        notEnoughMoneyCoroutine = null;
    }

    public void CardBuyingSetup(ShopItemView shopItemView, CardInShopWithPrice cardInShop) {
        canDragCompanions = false;
        selectingCompanionVeil.style.visibility = Visibility.Visible;
        selectingIndicator.style.visibility = Visibility.Visible;
        List<CompanionManagementView> notApplicable = new List<CompanionManagementView>();
        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            if (child.childCount != 1) continue;
            CompanionManagementView companion = visualElementToCompanionViewMap[child[0]];
            if (!shopManager.IsApplicableCompanion(cardInShop.sourceCompanion, companion.companion)) {
                notApplicable.Add(companion);
            }
        }

        foreach (VisualElement child in benchScrollView.contentContainer.hierarchy.Children()) {
            if (child.childCount != 1) continue;
            CompanionManagementView companion = visualElementToCompanionViewMap[child[0]];
            if (!shopManager.IsApplicableCompanion(cardInShop.sourceCompanion, companion.companion)) {
                notApplicable.Add(companion);
            }
        }
        
        foreach(CompanionManagementView view in notApplicable) {
            view.ShowNotApplicable();
        }
    }

    public void StopBuyingCard() {
        canDragCompanions = true;
        selectingCompanionVeil.style.visibility = Visibility.Hidden;
        selectingIndicator.style.visibility = Visibility.Hidden;
        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            if (child.childCount != 1) continue;
            visualElementToCompanionViewMap[child[0]].ResetApplicable();
        }

        foreach (VisualElement child in benchScrollView.contentContainer.hierarchy.Children()) {
            if (child.childCount != 1) continue;
            visualElementToCompanionViewMap[child[0]].ResetApplicable();
        }
    }

    private void CancelCardBuy(ClickEvent evt) {
        shopManager.ProcessCardBuyCanceled();
        StopBuyingCard();
    }

    // evt is unused, but required for the callback
    private void UpgradeButtonOnPointerEnter(PointerEnterEvent evt) {
        upgradeButtonTooltipCoroutine = UpgradeButtonTooltipCoroutine();
        StartCoroutine(upgradeButtonTooltipCoroutine);
    }

    // evt is unused, but required for the callback
    private void UpgradeButtonOnPointerLeave(PointerLeaveEvent evt) {
        if (upgradeButtonTooltipCoroutine != null) {
            StopCoroutine(upgradeButtonTooltipCoroutine);
            upgradeButtonTooltipCoroutine = null;
        }

        if (this.tooltip != null) {
            this.tooltip.RemoveFromHierarchy();
            this.tooltip = null;
        }
    }

    private IEnumerator UpgradeButtonTooltipCoroutine() {
        yield return new WaitForSeconds(1f);
        ShowUpgradeButtonTooltip();
    }

    private void ShowUpgradeButtonTooltip() {
        VisualElement tooltip = shopManager.GetShopLevel().upgradeTooltip.GetVisualElement();

        VisualElement background = new VisualElement();

        background.AddToClassList("shop-tooltip-background");

        background.Add(tooltip);
        uiDoc.rootVisualElement.Add(background);

        background.style.top = upgradeButton.worldBound.yMin;
        background.style.left = upgradeButton.worldBound.xMax + upgradeButton.worldBound.width / 3;

        this.tooltip = background;
    }

    public void SetShopRerollPrice(int amount) {
        rerollPriceLabel.text = "$" + amount.ToString();
    }

    public void SetShopUpgradePrice(int amount) {
        upgradePriceLabel.text = "$" + amount.ToString();
    }

    public void DisableUpgradeButton() {
        upgradeButton.SetEnabled(false);
        upgradeButton.UnregisterCallback<PointerEnterEvent>(UpgradeButtonOnPointerEnter);
    }

    public MonoBehaviour GetMonoBehaviour()
    {
        return this;
    }

    public void AddToRoot(VisualElement element)
    {
        uiDoc.rootVisualElement.Add(element);
    }

    public void ShowCompanionDeckView(Companion companion)
    {
        if (cardSelectionViewPrefab != null) {
            GameObject cardSelectionViewGo = Instantiate(cardSelectionViewPrefab);
            CardSelectionView cardSelectionView = cardSelectionViewGo.GetComponent<CardSelectionView>();
            cardSelectionView.Setup(companion.getDeck().cards, companion);
        } else {
            deckViewContentContainer.Clear();
            deckView.style.visibility = Visibility.Visible;

            foreach (Card card in companion.deck.cards) {
                CardView cardView = new CardView(card.cardType, companion.companionType, card.shopRarity, true);
                cardView.cardContainer.style.marginBottom = 10;
                cardView.cardContainer.style.marginLeft = 10;
                cardView.cardContainer.style.marginRight = 10;
                cardView.cardContainer.style.marginTop = 10;
                deckViewContentContainer.Add(cardView.cardContainer);
            }
        }
    }

    public void SellCompanion(CompanionManagementView companionView)
    {
        SellCompanionOnClick();
        sellingCompanionConfirmation.style.visibility = Visibility.Visible;
            this.companionToSell = companionView;
            Label confirmSellCompanionLabel = sellingCompanionConfirmation.Q<Label>("selling-companion-confirmation-label");
            string replacedText = String.Format(
                originalSellingCompanionConfirmationText, 
                companionView.companion.GetName(), 
                shopManager.CalculateCompanionSellPrice(companionView.companion));
            confirmSellCompanionLabel.text = replacedText;
    }

    private void CloseCompanionDeckView() {
        deckViewContentContainer.Clear();
        deckView.style.visibility = Visibility.Hidden;
    }

    public void ShowCantSellLastCompanion()
    {
        StartCoroutine(ShowGenericNotification("Unable to sell companion, this is your last one active!")); 
    }

    public void ShowCompanionUpgradedMessage(String companionName, int newLevel)
    {
        StartCoroutine(ShowGenericNotification(companionName + " has been upgraded to level " + newLevel + "!", 1.5f)); 
    }

    public IEnumerator ShowGenericNotification(string text, float time = 2.5f) {
        genericMessageBox.style.visibility = Visibility.Visible;
        genericMessageBox.Q<Label>().text = text;
        yield return new WaitForSeconds(2.5f);
        genericMessageBox.style.visibility = Visibility.Hidden;
    }
    public bool IsSellingCompanions() {
        return sellingCompanions;
    }

    public bool IsDraggingCompanion() {
        return isDraggingCompanion;
    }

    public void ShopItemViewHovered(ShopItemView shopItemView)
    {
        shopManager.ShopItemHovered();
    }

    public VisualElement GetUpgradeShopButton() {
        return upgradeButton;
    }

    public VisualElement GetRerollShopButton() {
        return rerollButton;
    }

    public VisualElement GetMoneyIndicator() {
        return moneyLabel;
    }  

    public void ShowCompanionUpgradeMenu(List<Companion> companions, Companion upgradeCompanion) {
        inUpgradeMenu = true;
        VisualElement upgradeMenuOuterContainer = uiDoc.rootVisualElement.Q<VisualElement>(name:"upgradeMenuOuterContainer");
        VisualElement companionUpgradeMenu = uiDoc.rootVisualElement.Q<VisualElement>(name:"upgradeMenuContainer");
        VisualElement initialCompanionContainer = uiDoc.rootVisualElement.Q<VisualElement>(name: "currentCompanions");
        VisualElement upgradeCompanionsContainer = uiDoc.rootVisualElement.Q<VisualElement>(name: "upgradedCompanion");
        initialCompanionContainer.Clear();
        upgradeCompanionsContainer.Clear();

        int delay = 500;
        for (int index = 0; index < companions.Count; index++) {
            var companion = companions[index];
            VisualElement companionContainer = new VisualElement();
            companionContainer.AddToClassList("victory-companion-container");
            companionContainer.AddToClassList("upgrade-menu-companion-invisible");
            
            CompanionTypeSO companionType = companion.companionType;
            EntityView entityView = new EntityView(companion, 0, false);
            VisualElement portraitContainer = entityView.entityContainer.Q(className: "portrait-container");
            portraitContainer.style.backgroundImage = new StyleBackground(companionType.sprite);
            companionContainer.Add(entityView.entityContainer);
            initialCompanionContainer.Add(companionContainer);
            
            int displayIndex = index + 1;
            companionContainer.schedule.Execute(() => {
                companionContainer.AddToClassList("upgrade-menu-compainion-1");
            }).StartingIn(delay);
            delay += 250;
        }

        VisualElement upgradeCompanionContainer = new VisualElement();
        upgradeCompanionContainer.AddToClassList("victory-companion-container");
        upgradeCompanionContainer.AddToClassList("upgrade-menu-companion-invisible");
        CompanionTypeSO upgradeCompanionType = upgradeCompanion.companionType;
        EntityView upgradeEntityView = new EntityView(upgradeCompanion, 0, false);
        //entityView.entityContainer.AddToClassList("compendium-item-container");
        VisualElement upgradePortraitContainer = upgradeEntityView.entityContainer.Q(className: "portrait-container");
        upgradePortraitContainer.style.backgroundImage = new StyleBackground(upgradeCompanionType.sprite);
        upgradeCompanionContainer.Add(upgradeEntityView.entityContainer);
        upgradeCompanionContainer.schedule.Execute(() => {
            upgradeCompanionContainer.AddToClassList("upgrade-menu-compainion-1");
        }).StartingIn(delay);
        upgradeCompanionsContainer.Add(upgradeCompanionContainer);
        companionUpgradeMenu.AddToClassList("upgrade-menu-container-visible");
        upgradeMenuOuterContainer.AddToClassList("upgrade-menu-outer-container-visible");
        upgradeMenuOuterContainer.pickingMode = PickingMode.Position;
    }

    private void CancelUpgrade() {
        if (!inUpgradeMenu) return;
        inUpgradeMenu = false;
        VisualElement upgradeMenuOuterContainer = uiDoc.rootVisualElement.Q<VisualElement>(name:"upgradeMenuOuterContainer");
        VisualElement companionUpgradeMenu = uiDoc.rootVisualElement.Q<VisualElement>(name:"upgradeMenuContainer");
        companionUpgradeMenu.RemoveFromClassList("upgrade-menu-container-visible");
        upgradeMenuOuterContainer.RemoveFromClassList("upgrade-menu-outer-container-visible");
        upgradeMenuOuterContainer.pickingMode = PickingMode.Ignore;
        shopManager.CancelUpgradePurchase();
    }

    private void ConfirmUpgrade() {
        if (!inUpgradeMenu) return;
        inUpgradeMenu = false;

        VisualElement upgradeMenuOuterContainer = uiDoc.rootVisualElement.Q<VisualElement>(name:"upgradeMenuOuterContainer");
        VisualElement companionUpgradeMenu = uiDoc.rootVisualElement.Q<VisualElement>(name:"upgradeMenuContainer");
        companionUpgradeMenu.RemoveFromClassList("upgrade-menu-container-visible");
        upgradeMenuOuterContainer.RemoveFromClassList("upgrade-menu-outer-container-visible");
        upgradeMenuOuterContainer.pickingMode = PickingMode.Ignore;
        shopManager.ConfirmUpgradePurchase();
    }

    public void DisplayTooltip(VisualElement element, TooltipViewModel tooltipViewModel, bool forCompanionManagementView) {
        Vector3 tooltipPosition = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(element);
        if (forCompanionManagementView) {
            tooltipPosition.x -= element.resolvedStyle.height / 100; // this feels super brittle
            tooltipPosition.y += element.resolvedStyle.height / 100;
        } else {
            tooltipPosition.x -= element.resolvedStyle.width / 120; // this feels super brittle 
            tooltipPosition.y += element.resolvedStyle.width / 150;
        }
        GameObject uiDocToolTipPrefab = Instantiate(shopManager.tooltipPrefab, tooltipPosition, new Quaternion());
        TooltipView tooltipView = uiDocToolTipPrefab.GetComponent<TooltipView>();
        tooltipView.tooltip = tooltipViewModel;

        tooltipMap[element] = uiDocToolTipPrefab;
    }

    public void DestroyTooltip(VisualElement element) {
        if(tooltipMap.ContainsKey(element)) {
            Destroy(tooltipMap[element]);
        }
    }
}
