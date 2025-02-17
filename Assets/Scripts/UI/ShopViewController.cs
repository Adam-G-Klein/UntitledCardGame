using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    private ShopManager shopManager;
    private Dictionary<CardInShopWithPrice, ShopItemView> cardItemToViewMap;
    private Dictionary<CompanionInShopWithPrice, ShopItemView> companionItemToViewMap;

    // Specific shop VisualElement references
    private VisualElement shopGoodsArea;
    private ScrollView benchScrollView;
    private VisualElement activeContainer;
    private VisualElement mapContainer;
    private Button upgradeButton;
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
    private List<VisualElement> blockedSlots = new List<VisualElement>();

    private IEnumerator notEnoughMoneyCoroutine;
    private IEnumerator upgradeButtonTooltipCoroutine = null;
    private VisualElement tooltip;
    private bool sellingCompanions = false;
    private Companion companionToSell;
    private string originalSellingCompanionConfirmationText;

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

        uiDoc.rootVisualElement.Q<Button>("reroll-button").clicked += RerollButtonOnClick;
        selectingCancelButton.clicked += CancelCardBuy;
        upgradeButton = uiDoc.rootVisualElement.Q<Button>("upgrade-button");
        upgradeButton.clicked += UpgradeButtonOnClick;
        upgradeButton.RegisterCallback<PointerEnterEvent>(UpgradeButtonOnPointerEnter);
        upgradeButton.RegisterCallback<PointerLeaveEvent>(UpgradeButtonOnPointerLeave);
        uiDoc.rootVisualElement.Q<Button>("start-next-combat-button").clicked += StartNextCombatOnClick;
        //sellCompanionButton.clicked += SellCompanionOnClick;
        sellingCompanionConfirmation.Q<Button>("selling-companion-confirmation-yes").clicked += ConfirmSellCompanion;
        sellingCompanionConfirmation.Q<Button>("selling-companion-confirmation-no").clicked += StopSellingCompanion;
        originalSellingCompanionConfirmationText = sellingCompanionConfirmation.Q<Label>("selling-companion-confirmation-label").text;
        deckView.Q<Button>().clicked += CloseCompanionDeckView;
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
                child.Clear();
            }
            child.style.backgroundColor = slotNotHighlightColor;
        }
        foreach (VisualElement child in benchScrollView.contentContainer.hierarchy.Children()) {
            if (child.childCount > 0) {
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
            shopManager.ProcessCompanionBuyRequestV2(shopItemView, shopItemView.companionInShop);
        } else if (shopItemView.cardInShop != null) {
            shopManager.ProcessCardBuyRequestV2(shopItemView, shopItemView.cardInShop);
        }
    }

    public void RerollButtonOnClick() {
        shopManager.ProcessRerollShopClick();
    }

    public void UpgradeButtonOnClick() {
        shopManager.ProcessUpgradeShopClick();
    }

    public void StartNextCombatOnClick() {
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
            //this.companionToSell = companionView;
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
        shopManager.SellCompanion(companionToSell);
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

    public void CompanionManagementOnPointerDown(CompanionManagementView companionView, PointerDownEvent evt)
    {
        if (!canDragCompanions && !isDraggingCompanion) return;

        VisualElement parent = companionView.container.parent;

        VisualElement tempContainer = new VisualElement();
        tempContainer.style.width = parent.resolvedStyle.width;
        tempContainer.style.height = parent.resolvedStyle.height;
        tempContainer.style.position = Position.Absolute;

        uiDoc.rootVisualElement.Add(tempContainer);
        StartCoroutine(FinishDragSetup(tempContainer, companionView.container, evt.position, parent));
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

    public void CompanionManagementOnPointerMove(CompanionManagementView companionManagementView, PointerMoveEvent evt)
    {        
        if (!isDraggingCompanion || companionBeingDragged != companionManagementView.container)  return;

        companionManagementView.container.parent.style.top = evt.position.y - companionManagementView.container.parent.layout.height / 2;
        companionManagementView.container.parent.style.left = evt.position.x - companionManagementView.container.parent.layout.width / 2;

        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            if (blockedSlots != null && blockedSlots.Contains(child)) continue;
            if (child.worldBound.Contains(evt.position)) {
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
            if (child.worldBound.Contains(evt.position)) {
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

            RefreshContainers(activeContainer.Children().ToList(), false);
            RefreshContainers(benchScrollView.contentContainer.Children().ToList(), true);
        // Scenario 2, dragging companion to slot with another companion in it already
        } else if (movingToContainer.childCount == 1) {
            originalParent.Add(movingToContainer[0]);
            VisualElement tempContainer = companionManagementView.container.parent;
            movingToContainer.Add(companionManagementView.container);
            tempContainer.RemoveFromHierarchy();
        } else {
            Debug.LogError("Companion container contains more than 1 element in heirarchy");
        }
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

    private void CancelCardBuy() {
        shopManager.ProcessCardBuyCanceled();
        StopBuyingCard();
    }

    private void UpgradeButtonOnPointerEnter(PointerEnterEvent evt) {
        upgradeButtonTooltipCoroutine = UpgradeButtonTooltipCoroutine();
        StartCoroutine(upgradeButtonTooltipCoroutine);
    }

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
        deckViewContentContainer.Clear();
        deckView.style.visibility = Visibility.Visible;

        foreach (Card card in companion.deck.cards) {
            CardView cardView = new CardView(card.cardType, companion.companionType, true);
            cardView.cardContainer.style.marginBottom = 10;
            cardView.cardContainer.style.marginLeft = 10;
            cardView.cardContainer.style.marginRight = 10;
            cardView.cardContainer.style.marginTop = 10;
            deckViewContentContainer.Add(cardView.cardContainer);
        }
    }

    public void SellCompanion(Companion companion)
    {
        SellCompanionOnClick();
        sellingCompanionConfirmation.style.visibility = Visibility.Visible;
            this.companionToSell = companion;
            Label confirmSellCompanionLabel = sellingCompanionConfirmation.Q<Label>("selling-companion-confirmation-label");
            string replacedText = String.Format(
                originalSellingCompanionConfirmationText, 
                companion.GetName(), 
                shopManager.CalculateCompanionSellPrice(companion));
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
}
