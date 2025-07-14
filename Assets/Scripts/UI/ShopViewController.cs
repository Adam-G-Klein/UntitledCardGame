using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopViewController : MonoBehaviour,
    IShopItemViewDelegate,
    ICompanionManagementViewDelegate,
    ISellingCompanionConfirmationViewDelegate,
    IControlsReceiver
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
    private VisualElement benchContainer;
    private VisualElement activeContainer;
    private VisualElement autoUpgradeToggle;
    private VisualElement mapContainer;
    private Button upgradeButton;
    private VisualElement upgradeIncrementContainer;
    private Button rerollButton;
    private Button cardRemovalButton;
    private Label moneyLabel;
    private Label notEnoughMoneyLabel;
    public VisualElement selectingCompanionVeil;
    public VisualElement selectingIndicator;
    public Button selectingCancelButton;
    private VisualElement selectingIndicatorForCardRemovalIndicator;
    private Button selectingForCardRemovalButton;
    private Button startNextCombatButton;

    public Label upgradePriceLabel;
    public Label rerollPriceLabel;
    public Label cardRemovalPriceLabel;
    public SellingCompanionConfirmationView sellingCompanionConfirmationView;
    private VisualElement deckView;
    private VisualElement deckViewContentContainer;
    private VisualElement genericMessageBox;

    // For dragging and dropping companions in the unit management
    private bool isDraggingCompanion = false;
    private CompanionManagementView companionBeingDragged = null;
    private CompanionManagementSlotView originalSlot = null;
    private List<CompanionManagementSlotView> activeSlots = new List<CompanionManagementSlotView>();
    private List<CompanionManagementSlotView> benchSlots = new List<CompanionManagementSlotView>();
    // This is the list of focusables to disable when you start dragging a companion around
    private List<VisualElementFocusable> disableOnCompanionDrag = new List<VisualElementFocusable>();
    private List<VisualElementFocusable> companionUpgradeFocusables = new List<VisualElementFocusable>();

    private IEnumerator notEnoughMoneyCoroutine;
    private IEnumerator upgradeButtonTooltipCoroutine = null;
    private VisualElement tooltip;
    private bool sellingCompanions = false;
    private CompanionManagementView companionToSell;
    private string originalSellingCompanionConfirmationText;
    private string originalSellingCompanionBreakdownText;
    private bool inUpgradeMenu = false;
    private Dictionary<VisualElement, GameObject> tooltipMap = new();
    private Companion currentUpgradeCompanion;

    private static string COMPANION_MANAGEMENT = "CompanionManagement";
    private static string UPGRADE_MENU = "UpgradeMenu";

    public void Start() {
        // Init(null);
        ControlsManager.Instance.RegisterControlsReceiver(this);
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
        benchContainer = uiDoc.rootVisualElement.Q("bench-container");
        moneyLabel = uiDoc.rootVisualElement.Q<Label>("money-indicator-label");
        notEnoughMoneyLabel = uiDoc.rootVisualElement.Q<Label>("not-enough-money-indicator");
        selectingCompanionVeil = uiDoc.rootVisualElement.Q("selecting-companion-veil");
        selectingIndicator = uiDoc.rootVisualElement.Q("companion-selection-indicator");
        selectingCancelButton = uiDoc.rootVisualElement.Q<Button>("companion-selection-cancel-button");
        upgradePriceLabel = uiDoc.rootVisualElement.Q<Label>("upgrade-price-label");
        rerollPriceLabel = uiDoc.rootVisualElement.Q<Label>("reroll-price-label");
        cardRemovalPriceLabel = uiDoc.rootVisualElement.Q<Label>("card-remove-price-label");

        sellingCompanionConfirmationView = new SellingCompanionConfirmationView(uiDoc.rootVisualElement.Q("selling-companion-confirmation"), this);

        deckView = uiDoc.rootVisualElement.Q("deck-view");
        deckViewContentContainer = uiDoc.rootVisualElement.Q("deck-view-card-area");
        genericMessageBox = uiDoc.rootVisualElement.Q("generic-message-box");

        SetupActiveSlots();
        SetBlockedActiveSlotsIfNecessary(shopManager.gameState.companions.currentCompanionSlots);
        SetupBenchSlots();

        rerollButton = uiDoc.rootVisualElement.Q<Button>("reroll-button");
        rerollButton.RegisterOnSelected(RerollButtonOnClick);
        FocusManager.Instance.RegisterFocusableTarget(rerollButton.AsFocusable());
        disableOnCompanionDrag.Add(rerollButton.AsFocusable());

        selectingCancelButton.RegisterOnSelected(CancelCardBuy);
        FocusManager.Instance.RegisterFocusableTarget(selectingCancelButton.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(selectingCancelButton.AsFocusable());

        upgradeButton = uiDoc.rootVisualElement.Q<Button>("upgrade-button");
        upgradeButton.RegisterOnSelected(UpgradeButtonOnClick);
        upgradeButton.RegisterCallback<PointerEnterEvent>(UpgradeButtonOnPointerEnter);
        upgradeButton.RegisterCallback<PointerLeaveEvent>(UpgradeButtonOnPointerLeave);
        upgradeIncrementContainer = uiDoc.rootVisualElement.Q("upgrade-increment-container");
        VisualElementFocusable upgradeButtonFocusable = upgradeButton.AsFocusable();
        upgradeButtonFocusable.additionalFocusAction += () => UpgradeButtonOnPointerEnter(null);
        upgradeButtonFocusable.additionalUnfocusAction += () => UpgradeButtonOnPointerLeave(null);
        FocusManager.Instance.RegisterFocusableTarget(upgradeButtonFocusable);
        disableOnCompanionDrag.Add(upgradeButtonFocusable);

        startNextCombatButton = uiDoc.rootVisualElement.Q<Button>("start-next-combat-button");
        startNextCombatButton.RegisterOnSelected(StartNextCombatOnClick);
        FocusManager.Instance.RegisterFocusableTarget(startNextCombatButton.AsFocusable());
        disableOnCompanionDrag.Add(startNextCombatButton.AsFocusable());

        Button closeCompanionDeckViewButton = deckView.Q<Button>();
        closeCompanionDeckViewButton.RegisterOnSelected((evt) => CloseCompanionDeckView());
        FocusManager.Instance.RegisterFocusableTarget(closeCompanionDeckViewButton.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(closeCompanionDeckViewButton.AsFocusable());

        cardRemovalButton = uiDoc.rootVisualElement.Q<Button>("card-remove-button");
        cardRemovalButton.RegisterOnSelected(CardRemovalButtonOnClick);
        FocusManager.Instance.RegisterFocusableTarget(cardRemovalButton.AsFocusable());
        disableOnCompanionDrag.Add(cardRemovalButton.AsFocusable());

        selectingIndicatorForCardRemovalIndicator = uiDoc.rootVisualElement.Q<VisualElement>("companion-selection-for-card-removal-indicator");
        selectingForCardRemovalButton = uiDoc.rootVisualElement.Q<Button>("companion-selection-for-card-removal-cancel-button");
        selectingForCardRemovalButton.RegisterOnSelected(CancelCardRemoval);
        FocusManager.Instance.RegisterFocusableTarget(selectingForCardRemovalButton.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(selectingForCardRemovalButton.AsFocusable());

        // setup upgradeMenu
        Button cancelUpgradeButton = uiDoc.rootVisualElement.Q<Button>(name:"cancelUpgrade");
        Button confirmUpgradeButton = uiDoc.rootVisualElement.Q<Button>(name:"confirmUpgrade");
        Button upgradedDeckPreviewButton = uiDoc.rootVisualElement.Q<Button>(name:"upgradedDeckPreview");
        cancelUpgradeButton.RegisterOnSelected((evt) => CancelUpgrade());
        confirmUpgradeButton.RegisterOnSelected((evt) => ConfirmUpgrade());
        upgradedDeckPreviewButton.RegisterOnSelected((evt) => PreviewUpgradedDeck());
        companionUpgradeFocusables.Add(cancelUpgradeButton.AsFocusable());
        companionUpgradeFocusables.Add(confirmUpgradeButton.AsFocusable());
        companionUpgradeFocusables.Add(upgradedDeckPreviewButton.AsFocusable());

        VisualElement questionMark = uiDoc.rootVisualElement.Q<VisualElement>(name:"questionMark");
        questionMark.RegisterCallback<PointerEnterEvent>(ShowHelperText);
        questionMark.RegisterCallback<PointerLeaveEvent>(HideHelperText);
        VisualElementFocusable questionMarkFocusable = questionMark.AsFocusable();
        questionMarkFocusable.additionalFocusAction += () => ShowHelperText(null);
        questionMarkFocusable.additionalUnfocusAction += () => HideHelperText(null);
        companionUpgradeFocusables.Add(questionMarkFocusable);
        autoUpgradeToggle = uiDoc.rootVisualElement.Q("auto-upgrade-toggle");
        autoUpgradeToggle.RegisterOnSelected(ToggleAutoUpgrade);
        VisualElementFocusable autoUpgradeToggleFocusable = autoUpgradeToggle.AsFocusable();
        companionUpgradeFocusables.Add(autoUpgradeToggleFocusable);

        companionUpgradeFocusables.ForEach((focusable) => {
            FocusManager.Instance.RegisterFocusableTarget(focusable);
            FocusManager.Instance.DisableFocusableTarget(focusable);
        });
    }

    private void PreviewUpgradedDeck() {
        ShowCompanionDeckView(currentUpgradeCompanion);
    }

    private void ShowHelperText(PointerEnterEvent evt) {
        uiDoc.rootVisualElement.Q<VisualElement>(name:"explainerText").AddToClassList("explainer-text-container-visible");
    }

    private void HideHelperText(PointerLeaveEvent evt) {
        uiDoc.rootVisualElement.Q<VisualElement>(name:"explainerText").RemoveFromClassList("explainer-text-container-visible");
    }

    private void ToggleAutoUpgrade(ClickEvent evt) {
        shopManager.SetAutoUpgrade((evt.target as Toggle).value);
    }

    private void SetupActiveSlots() {
        foreach (VisualElement child in activeContainer.hierarchy.Children()) {
            CompanionManagementSlotView slotView = new CompanionManagementSlotView(child,
                slotNotHighlightColor,
                slotHighlightColor,
                slotUnavailableColor);
            activeSlots.Add(slotView);
        }
    }

    private void SetBlockedActiveSlotsIfNecessary(int numCompanions) {
        if (numCompanions >= 5) return;
        int i = 0;
        foreach (CompanionManagementSlotView slotView in activeSlots) {
            i++;
            if (i > numCompanions) {
                slotView.SetBlocked();
            }
        }
    }

    private void SetupBenchSlots() {
        foreach (VisualElement child in benchContainer.hierarchy.Children()) {
            CompanionManagementSlotView slotView = new CompanionManagementSlotView(child,
                slotNotHighlightColor,
                slotHighlightColor,
                slotUnavailableColor);
            benchSlots.Add(slotView);
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

    public void SetupUpgradeIncrements() {
        upgradeIncrementContainer.Clear();
        int incrementsToUnlock = shopManager.GetShopLevel().shopLevelIncrementsToUnlock;
        for (int i = 0; i < incrementsToUnlock; i++) {
            VisualElement newIncrement = new();
            newIncrement.AddToClassList("upgradeIncrement");
            if (shopManager.gameState.playerData.GetValue().shopLevelIncrementsEarned > i) {
                newIncrement.AddToClassList("upgradeIncrementEarned");
            }
            upgradeIncrementContainer.Add(newIncrement);
        }
    }

    public void ActivateUpgradeIncrement(int upgradeIncrementIndex) {
        upgradeIncrementContainer.Children().ToList()[upgradeIncrementIndex].AddToClassList("upgradeIncrementEarned");
    }

    public void Clear() {
        ClearUnitManagement();
        shopGoodsArea.Clear();
        SetBlockedActiveSlotsIfNecessary(shopManager.gameState.companions.currentCompanionSlots);
    }

    public void AddCardToShopView(CardInShopWithPrice card) {
        ShopItemView newCardItemView = new ShopItemView(this, card);

        shopGoodsArea.Add(newCardItemView.shopItemElement);

        cardItemToViewMap.Add(card, newCardItemView);

        FocusManager.Instance.RegisterFocusableTarget(newCardItemView.visualElementFocusable);
        disableOnCompanionDrag.Add(newCardItemView.visualElementFocusable);
    }

    public void RemoveCardFromShopView(CardInShopWithPrice card) {
        ShopItemView shopItemView = cardItemToViewMap[card];

        // TODO: Replace with sold out? Grey it out? Talk to Jasmine
        shopItemView.Disable();

        cardItemToViewMap.Remove(card);

        FocusManager.Instance.UnregisterFocusableTarget(shopItemView.visualElementFocusable);
        disableOnCompanionDrag.Remove(shopItemView.visualElementFocusable);
    }

    public void AddCompanionToShopView(CompanionInShopWithPrice companion) {
        ShopItemView newCompanionItemView = new ShopItemView(this, companion);

        shopGoodsArea.Add(newCompanionItemView.shopItemElement);

        companionItemToViewMap.Add(companion, newCompanionItemView);

        FocusManager.Instance.RegisterFocusableTarget(newCompanionItemView.visualElementFocusable);
        disableOnCompanionDrag.Add(newCompanionItemView.visualElementFocusable);
    }

    public void RemoveCompanionFromShopView(CompanionInShopWithPrice companion) {
        ShopItemView shopItemView = companionItemToViewMap[companion];

        // TODO: Replace with sold out? Grey it out? Talk to Jasmine
        shopItemView.Disable();
        companionItemToViewMap.Remove(companion);
        FocusManager.Instance.UnregisterFocusableTarget(shopItemView.visualElementFocusable);
        disableOnCompanionDrag.Remove(shopItemView.visualElementFocusable);
    }

    public void RebuildUnitManagement(CompanionListVariableSO companionList)
    {
        ClearUnitManagement();
        SetBlockedActiveSlotsIfNecessary(shopManager.gameState.companions.currentCompanionSlots);
        SetupActiveCompanions(companionList.activeCompanions);
        SetupBenchCompanions(companionList.benchedCompanions);
    }

    private void ClearUnitManagement() {
        foreach (CompanionManagementSlotView slotView in activeSlots) {
            slotView.Reset();
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            slotView.Reset();
        }
    }

    public void SetupActiveCompanions(List<Companion> companions) {
        for (int i = 0; i < companions.Count; i++) {
            CompanionManagementView companionView = new CompanionManagementView(companions[i], this);
            activeSlots[i].InsertCompanion(companionView);
        }
    }

    public void SetupBenchCompanions(List<Companion> companions) {

        for (int i = 0; i < companions.Count; i++) {
            CompanionManagementView companionView = new CompanionManagementView(companions[i], this);
            benchSlots[i].InsertCompanion(companionView);
        }
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
            StopSellingCompanion();
        } else {
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
        sellingCompanionConfirmationView.Hide();
    }

    public void CompanionManagementOnClick(CompanionManagementView companionView)
    {
        shopManager.ProcessCompanionClicked(companionView.companion);
    }

    public void ConfirmSellCompanion() {
        shopManager.SellCompanion(companionToSell.companion, companionToSell.container);
        companionToSell = null;
        sellingCompanions = false;
        canDragCompanions = true;
        selectingCompanionVeil.style.visibility = Visibility.Hidden;
        sellingCompanionConfirmationView.Hide();
    }

    private void DontSellCompanion() {
        companionToSell = null;
        sellingCompanionConfirmationView.Hide();
    }

    public void CompanionManagementOnPointerDown(CompanionManagementView companionView, Vector2 pointerPos)
    {
        if (!canDragCompanions && !isDraggingCompanion) return;

        CompanionManagementSlotView parentSlot = GetParentSlotViewForCompanion(companionView);
        if (parentSlot == null) {
            Debug.LogWarning("RESETTING COMPANION MANAGEMENT VIEW");
            // Somehow something broke in a spectacular way, so lets just reset the view
            // s o f t l o c k     p r e v e n t i o n  (an attempt anyway)
            RebuildUnitManagement(shopManager.gameState.companions);
            return;
        }

        VisualElement parent = companionView.container.parent;

        VisualElement tempContainer = new VisualElement();
        tempContainer.style.width = parent.resolvedStyle.width;
        tempContainer.style.height = parent.resolvedStyle.height;
        tempContainer.style.position = Position.Absolute;

        uiDoc.rootVisualElement.Add(tempContainer);
        StartCoroutine(FinishDragSetup(tempContainer, companionView, pointerPos, parentSlot));
    }

    private CompanionManagementSlotView GetParentSlotViewForCompanion(CompanionManagementView companionView) {
        foreach (CompanionManagementSlotView slotView in activeSlots) {
            if (slotView.companionManagementView == companionView) return slotView;
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (slotView.companionManagementView == companionView) return slotView;
        }

        Debug.LogError("Could not find parent slot for companion!");
        return null;
    }

    private IEnumerator FinishDragSetup(VisualElement tempContainer, CompanionManagementView companion, Vector2 position, CompanionManagementSlotView originalSlot) {
        yield return new WaitForEndOfFrame();
        tempContainer.style.top = position.y - tempContainer.layout.height / 2;
        tempContainer.style.left = position.x - tempContainer.layout.width / 2;
        this.originalSlot = originalSlot;
        originalSlot.RemoveCompanion();
        originalSlot.SetHighlighted();
        tempContainer.Add(companion.container);
        isDraggingCompanion = true;
        companionBeingDragged = companion;
        disableOnCompanionDrag.ForEach((item) => FocusManager.Instance.StashFocusableTarget(this.GetType().Name + COMPANION_MANAGEMENT, item));
        CompanionManagementSlotView.MakeAllFocusable(activeSlots);
        CompanionManagementSlotView.MakeAllFocusable(benchSlots);
    }

    public void CompanionManagementOnPointerMove(CompanionManagementView companionManagementView, Vector2 pointerPos)
    {
        if (!isDraggingCompanion || companionBeingDragged != companionManagementView) {
            return;
        }

        companionManagementView.container.parent.style.top = pointerPos.y - companionManagementView.container.parent.layout.height / 2;
        companionManagementView.container.parent.style.left = pointerPos.x - companionManagementView.container.parent.layout.width / 2;

        foreach (CompanionManagementSlotView slotView in activeSlots) {
            if (slotView.IsBlocked()) continue;
            if (slotView.ContainsPosition(pointerPos)) {
                if (NumTakenSlots(activeSlots) < 5) {
                    MoveWhileDragging(activeSlots, slotView);
                    originalSlot = slotView;
                }
                slotView.SetHighlighted();
            } else {
                slotView.SetNotHighlighted();
            }
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (slotView.ContainsPosition(pointerPos)) {
                if (NumTakenSlots(benchSlots) < 5) {
                    MoveWhileDragging(benchSlots, slotView);
                    originalSlot = slotView;
                }
                slotView.SetHighlighted();
            } else {
                slotView.SetNotHighlighted();
            }
        }
    }

    // only used for mouse controls
    private void MoveWhileDragging(List<CompanionManagementSlotView> slots, CompanionManagementSlotView slotOver) {
        // pull all of the existing companions into a list and then iterate over the available slots in activecontainer. skip over the element over
        List<CompanionManagementView> companions = new List<CompanionManagementView>();
        foreach (CompanionManagementSlotView slotView in slots) {
            if (!slotView.IsEmpty()) {
                companions.Add(slotView.companionManagementView);
            }
        }

        if (companions.Count() == 0) {
            RefreshContainers(slots);
            return;
        }
        int companionIndex = 0;
        for (int i = 0; i < slots.Count(); i++) {
            CompanionManagementSlotView slotView = slots[i];
            slotView.RemoveCompanion();
            if (slotView == slotOver) continue;
            slotView.InsertCompanion(companions[companionIndex++]);
            if (companionIndex == companions.Count) {
                if (!(i == slots.Count() - 1)) {
                    i += 1;
                    slots[i].RemoveCompanion();
                }
                break;
            }
        }
        if (slots == activeSlots) {
            RefreshContainers(benchSlots);
        } else {
            RefreshContainers(activeSlots);
        }
    }

    public void CompanionManagementOnPointerLeave(CompanionManagementView companionManagementView, PointerLeaveEvent evt) {
        if (!isDraggingCompanion || companionBeingDragged != companionManagementView)  return;

        if(evt != null || NonMouseInputManager.Instance.inputMethod == InputMethod.Mouse) {
            companionManagementView.container.parent.style.top = evt.position.y - companionManagementView.container.parent.layout.height / 2;
            companionManagementView.container.parent.style.left = evt.position.x - companionManagementView.container.parent.layout.width / 2;
        }
    }

    public void ComapnionManagementOnPointerUp(CompanionManagementView companionManagementView, Vector2 pointerPos)
    {
        if (!isDraggingCompanion || companionManagementView != companionBeingDragged) return;

        CompanionManagementSlotView slotOver = null;
        foreach (CompanionManagementSlotView slotView in activeSlots) {
            if (slotView.ContainsPosition(pointerPos)) {
                slotOver = slotView;
            }
        }
        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (slotView.ContainsPosition(pointerPos)) {
                slotOver = slotView;
            }
        }

        if (slotOver != null && !slotOver.IsBlocked()) {
            DoMoveCompanion(companionManagementView, slotOver);
            slotOver.SetNotHighlighted();
        } else {
            VisualElement tempContainer = companionManagementView.container.parent;
            originalSlot.InsertCompanion(companionManagementView);
            uiDoc.rootVisualElement.Remove(tempContainer);
        }
        isDraggingCompanion = false;
        companionBeingDragged = null;
        originalSlot = null;
    }

    private void DoMoveCompanion(CompanionManagementView companionManagementView, CompanionManagementSlotView movingToSlot) {
        // Scenario 1, dragging companion to open container
        if (movingToSlot.IsEmpty()) {
            VisualElement tempContainer = companionManagementView.container.parent;

            // SPECIAL CASE ALERT: SPECIAL CASE A L E R T
            // Trying to move last active companion to *possibly* the bench
            if (!shopManager.CanMoveCompanionToNewOpenSlot(companionManagementView.companion)) {
                originalSlot.InsertCompanion(companionManagementView);
                uiDoc.rootVisualElement.Remove(tempContainer);
            } else {
                movingToSlot.InsertCompanion(companionManagementView);
                tempContainer.RemoveFromHierarchy();
            }
        // Scenario 2, dragging companion to slot with another companion in it already
        } else if (!movingToSlot.IsEmpty()) {
            originalSlot.InsertCompanion(movingToSlot.companionManagementView);
            VisualElement tempContainer = companionManagementView.container.parent;
            movingToSlot.InsertCompanion(companionManagementView);
            tempContainer.RemoveFromHierarchy();
        } else {
            Debug.LogError("Companion container contains more than 1 element in heirarchy");
        }
        RefreshContainers(activeSlots);
        RefreshContainers(benchSlots);
        SetCompanionOrdering();
        FocusManager.Instance.UnstashFocusables(this.GetType().Name + COMPANION_MANAGEMENT);
        CompanionManagementSlotView.ResetFocusable(activeSlots);
        CompanionManagementSlotView.ResetFocusable(benchSlots);
    }

    private int NumTakenSlots(List<CompanionManagementSlotView> slots) {
        int takenSlots = 0;
        foreach (CompanionManagementSlotView slotView in slots) {
            if (slotView.IsBlocked()) {
                takenSlots += 1;
            }
            else if (!slotView.IsEmpty()) {
                takenSlots += 1;
            }
        }
        return takenSlots;
    }

    private void SetCompanionOrdering() {
        List<Companion> activeCompanions = new List<Companion>();
        activeSlots.ForEach((slot) => {
            if (!slot.IsEmpty()) activeCompanions.Add(slot.companionManagementView.companion);
        });
        List<Companion> benchCompanions = new List<Companion>();
        benchSlots.ForEach((slot) => {
            if (!slot.IsEmpty()) benchCompanions.Add(slot.companionManagementView.companion);
        });
        shopManager.SetCompanionOrdering(activeCompanions, benchCompanions);
    }

    private void RefreshContainers(List<CompanionManagementSlotView> slots) {
        List<CompanionManagementView> companions = new List<CompanionManagementView>();
        foreach (CompanionManagementSlotView slot in slots) {
            if (!slot.IsEmpty()) {
                companions.Add(slot.companionManagementView);
                slot.RemoveCompanion();
            }
        }

        for (int i = 0; i < companions.Count; i++) {
            slots[i].InsertCompanion(companions[i]);
        }
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
    }

    public int GetBlockedCompanionSlots() {
        int num = 0;
        foreach (CompanionManagementSlotView slotView in activeSlots) {
            if (slotView.IsBlocked()) num += 1;
        }
        return num;
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
        StashNonCompanionViewFocusables(this.GetType().Name + "CardBuying");
        FocusManager.Instance.EnableFocusableTarget(selectingCancelButton.AsFocusable());
        List<CompanionManagementView> notApplicable = new List<CompanionManagementView>();
        foreach (CompanionManagementSlotView slotView in activeSlots) {
            if (slotView.IsEmpty()) continue;
            CompanionManagementView companion = slotView.companionManagementView;
            if (!shopManager.IsApplicableCompanion(cardInShop, companion.companion)) {
                notApplicable.Add(companion);
                FocusManager.Instance.StashFocusableTarget(this.GetType().Name + "CardBuying", GetParentSlotViewForCompanion(companion).veFocusable);
            }
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (slotView.IsEmpty()) continue;
            CompanionManagementView companion = slotView.companionManagementView;
            if (!shopManager.IsApplicableCompanion(cardInShop, companion.companion)) {
                notApplicable.Add(companion);
                FocusManager.Instance.StashFocusableTarget(this.GetType().Name + "CardBuying", GetParentSlotViewForCompanion(companion).veFocusable);
            }
        }

        foreach(CompanionManagementView view in notApplicable) {
            view.ShowNotApplicable();
        }
    }

    private void StashNonCompanionViewFocusables(string stashedBy) {
        FocusManager.Instance.StashFocusableTarget(stashedBy, upgradeButton.AsFocusable());
        FocusManager.Instance.StashFocusableTarget(stashedBy, rerollButton.AsFocusable());
        FocusManager.Instance.StashFocusableTarget(stashedBy, cardRemovalButton.AsFocusable());
        FocusManager.Instance.StashFocusableTarget(stashedBy, startNextCombatButton.AsFocusable());
        shopGoodsArea.Query<VisualElement>(className: "focusable").ToList().ForEach(ve => {
            FocusManager.Instance.StashFocusableTarget(stashedBy, ve.AsFocusable());
        });
    }

    public void StopBuyingCard() {
        canDragCompanions = true;
        selectingCompanionVeil.style.visibility = Visibility.Hidden;
        selectingIndicator.style.visibility = Visibility.Hidden;
        FocusManager.Instance.DisableFocusableTarget(selectingCancelButton.AsFocusable());
        FocusManager.Instance.UnstashFocusables(this.GetType().Name + "CardBuying");
        foreach (CompanionManagementSlotView slotView in activeSlots) {
            if (slotView.IsEmpty()) continue;
            slotView.companionManagementView.ResetApplicable();
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (slotView.IsEmpty()) continue;
            slotView.companionManagementView.ResetApplicable();
        }
    }

    private void CancelCardBuy(ClickEvent evt) {
        shopManager.ProcessCardBuyCanceled();
        StopBuyingCard();
    }

    public void CardRemovalButtonOnClick(ClickEvent evt) {
        shopManager.ProcessCardRemovalClick();
    }

    public void StartCardRemoval() {
        canDragCompanions = false;
        selectingCompanionVeil.style.visibility = Visibility.Visible;
        selectingIndicatorForCardRemovalIndicator.style.visibility = Visibility.Visible;
    }


    public void CardRemoved() {
        cardRemovalButton.SetEnabled(false);
        StartCoroutine(ShowGenericNotification("Card Removed!"));
        StopRemovingCard();
    }

    private void StopRemovingCard() {
        canDragCompanions = true;
        selectingCompanionVeil.style.visibility = Visibility.Hidden;
        selectingIndicatorForCardRemovalIndicator.style.visibility = Visibility.Hidden;
    }


    private void CancelCardRemoval() {
        shopManager.ProcessCardRemovalCancelled();
        StopRemovingCard();
    }

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
        VisualElement tooltip = shopManager.GetShopUpgradeTooltip().GetVisualElement();

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

    public void SetShopCardRemovalPrice(int amount) {
        cardRemovalPriceLabel.text = "$" + amount.ToString();
    }

    public void DisableUpgradeButton()
    {
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
            // Card Selection View stashes focusables on setup
            cardSelectionView.Setup(companion.getDeck().cards, companion);
        } else { // Not sure why this else exists
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

    public void ShopDeckViewForCardRemoval(Companion companion) {
        if (cardSelectionViewPrefab != null) {
            GameObject cardSelectionViewGo = Instantiate(cardSelectionViewPrefab);
            CardSelectionView cardSelectionView = cardSelectionViewGo.GetComponent<CardSelectionView>();
            cardSelectionView.Setup(companion.getDeck().cards, "Select a Card For Removal!", 1, 1, companion);
            cardSelectionView.cardsSelectedHandler += ProcessCardRemoval;
        }
    }

    private void ProcessCardRemoval(List<Card> cards, Companion companion) {
        cards.ForEach((card) => {
            companion.deck.PurgeCard(card.id);
            companion.trackingStats.RecordCardRemoval();
        });

        shopManager.ProcessCardRemoval();
        CardRemoved();
    }

    public void SellCompanion(CompanionManagementView companionView)
    {
        SellCompanionOnClick();
        this.companionToSell = companionView;
        sellingCompanionConfirmationView.Show(companionView);
    }

    public CompanionSellValue CalculateCompanionSellPrice(Companion companion) {
        return shopManager.CalculateCompanionSellPrice(companion);
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

    public bool CanDragCompanions() {
        return canDragCompanions;
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
        currentUpgradeCompanion = upgradeCompanion;
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
            VisualElement portraitContainer = entityView.entityContainer.Q(className: "entity-portrait");
            portraitContainer.style.backgroundImage = new StyleBackground(companionType.sprite);
            companionContainer.Add(entityView.entityContainer);
            initialCompanionContainer.Add(companionContainer);

            int displayIndex = index + 1;
            companionContainer.schedule.Execute(() => {
                companionContainer.AddToClassList("upgrade-menu-compainion-1");
            }).StartingIn(delay);
            entityView.entityContainer.RegisterCallback<PointerEnterEvent>((evt) => {
                DisplayTooltip(entityView.entityContainer, companion.companionType.tooltip, false);
            });
            entityView.entityContainer.RegisterCallback<PointerLeaveEvent>((evt) => {
                DestroyTooltip(entityView.entityContainer);
            });
            delay += 250;
        }

        VisualElement upgradeCompanionContainer = new VisualElement();
        upgradeCompanionContainer.AddToClassList("victory-companion-container");
        upgradeCompanionContainer.AddToClassList("upgrade-menu-companion-invisible");
        CompanionTypeSO upgradeCompanionType = upgradeCompanion.companionType;
        EntityView upgradeEntityView = new EntityView(upgradeCompanion, 0, false);
        //entityView.entityContainer.AddToClassList("compendium-item-container");
        VisualElement upgradePortraitContainer = upgradeEntityView.entityContainer.Q(className: "entity-portrait");
        upgradePortraitContainer.style.backgroundImage = new StyleBackground(upgradeCompanionType.sprite);
        upgradeCompanionContainer.Add(upgradeEntityView.entityContainer);
        upgradeCompanionContainer.schedule.Execute(() => {
            upgradeCompanionContainer.AddToClassList("upgrade-menu-compainion-1");
        }).StartingIn(delay);
        upgradeCompanionContainer.schedule.Execute(() => {
            // update hoverable pos after animation
            // UIDocumentHoverableInstantiator.Instance.UpdateHoverablesPosition();
        }).StartingIn(delay + 1000); // GET RACE CONDITIONED LOSER
        upgradeEntityView.entityContainer.RegisterCallback<PointerEnterEvent>((evt) => {
            DisplayTooltip(upgradeEntityView.entityContainer, upgradeCompanionType.tooltip, false);
        });
        upgradeEntityView.entityContainer.RegisterCallback<PointerLeaveEvent>((evt) => {
            DestroyTooltip(upgradeEntityView.entityContainer);
        });
        upgradeCompanionsContainer.Add(upgradeCompanionContainer);
        companionUpgradeMenu.AddToClassList("upgrade-menu-container-visible");
        upgradeMenuOuterContainer.AddToClassList("upgrade-menu-outer-container-visible");
        upgradeMenuOuterContainer.pickingMode = PickingMode.Position;
        FocusManager.Instance.StashFocusables(this.GetType().Name + UPGRADE_MENU);
        companionUpgradeFocusables.ForEach((focusable) => FocusManager.Instance.EnableFocusableTarget(focusable));
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
        FocusManager.Instance.UnstashFocusables(this.GetType().Name + UPGRADE_MENU);
        companionUpgradeFocusables.ForEach((focusable) => FocusManager.Instance.DisableFocusableTarget(focusable));
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
        FocusManager.Instance.UnstashFocusables(this.GetType().Name + UPGRADE_MENU);
        companionUpgradeFocusables.ForEach((focusable) => FocusManager.Instance.DisableFocusableTarget(focusable));
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

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        // Just need the IControlsReceiver for the SwappedControlMethod
        return;
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        if (controlMethod == ControlsManager.ControlMethod.Mouse && isDraggingCompanion) {
            // abort dragging companion in a *nice* way
            VisualElement tempContainer = companionBeingDragged.container.parent;
            originalSlot.InsertCompanion(companionBeingDragged);
            originalSlot.SetNotHighlighted();
            uiDoc.rootVisualElement.Remove(tempContainer);
            isDraggingCompanion = false;
            companionBeingDragged = null;
            originalSlot = null;
        }
    }

    public void DisplayCards(CompanionTypeSO companion)
    {
        List<Card> instantiatedCards = companion.startingDeck.cards
            .Select(card => new Card(card, companion))
            .ToList();

        GameObject cardSelectionViewGo = Instantiate(cardSelectionViewPrefab);
        CardSelectionView cardSelectionView = cardSelectionViewGo.GetComponent<CardSelectionView>();
        // Card Selection View stashes focusables on setup
        cardSelectionView.Setup(instantiatedCards, new Companion(companion));
    }
}
