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
    [SerializeField]
    private StatusEffectsSO statusEffectsSO;
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
    private Button cancelCardRemovalButton;
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
    private List<(CompanionView, CompanionTypeSO, VisualElement)> companionUpgradeInstanceFocusables = new List<(CompanionView, CompanionTypeSO, VisualElement)>();

    [Header("Card Buying Animation Values")]
    [SerializeField] private AnimationCurve cardBuyVFXHorizontal;
    [SerializeField] private AnimationCurve cardBuyVFXVertical;
    [SerializeField] private AnimationCurve cardBuyVFXRotation;
    [SerializeField] private AnimationCurve cardBuyVFXScale;
    [SerializeField] private float cardBuyVFXTime;
    [SerializeField] private AnimationCurve companionBuyMoveVFX;
    [SerializeField] private float companionBuyVFXTime;

    [Header("Companion Buying Animation Values")]
    [SerializeField] private float companionBuyTime = .175f;
    [SerializeField] private float companionUpgradeTime = .25f;
    [SerializeField] private float companionBuyScaleBump = 1.5f;
    [Header("Reroll Animation Values")]
    [SerializeField] private AnimationCurve oldItemsCurve;
    [SerializeField] private AnimationCurve newItemsCurve;
    [SerializeField] private float rerollVfxTime;
    [SerializeField] private float oldToNewDelay;
    [SerializeField] private float rerollTimeBetweenItems = .1f;

    private IEnumerator notEnoughMoneyCoroutine;
    private IEnumerator upgradeButtonTooltipCoroutine = null;
    private VisualElement tooltip;
    private bool sellingCompanions = false;
    public bool isBuyingDisabled = false;
    private CompanionManagementView companionToSell;
    private string originalSellingCompanionConfirmationText;
    private string originalSellingCompanionBreakdownText;
    private bool inUpgradeMenu = false;
    private Companion currentUpgradeCompanion;

    private static string COMPANION_MANAGEMENT = "CompanionManagement";
    private static string UPGRADE_MENU = "UpgradeMenu";
    private IEnumerator waitAndHideMessageCoroutine;
    private TooltipController tooltipController;

    public void Start()
    {
        // Init(null);
        ControlsManager.Instance.RegisterControlsReceiver(this);
    }

    public void Init(ShopManager shopManager) {
        if (uiDoc == null) {
            uiDoc = GetComponent<UIDocument>();
        }

        this.shopManager = shopManager;

        this.tooltipController = new TooltipController(shopManager.tooltipPrefab);

        cardItemToViewMap = new Dictionary<CardInShopWithPrice, ShopItemView>();
        companionItemToViewMap = new Dictionary<CompanionInShopWithPrice, ShopItemView>();

        mapContainer = uiDoc.rootVisualElement.Q("mapContainer");
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
        cancelCardRemovalButton = uiDoc.rootVisualElement.Q<Button>("companion-selection-for-card-removal-cancel-button");
        cancelCardRemovalButton.RegisterOnSelected(CancelCardRemoval);
        FocusManager.Instance.RegisterFocusableTarget(cancelCardRemovalButton.AsFocusable());
        FocusManager.Instance.DisableFocusableTarget(cancelCardRemovalButton.AsFocusable());

        // setup upgradeMenu
        Button cancelUpgradeButton = uiDoc.rootVisualElement.Q<Button>(name: "cancelUpgrade");
        Button confirmUpgradeButton = uiDoc.rootVisualElement.Q<Button>(name: "confirmUpgrade");
        Button upgradedDeckPreviewButton = uiDoc.rootVisualElement.Q<Button>(name: "upgradedDeckPreview");
        cancelUpgradeButton.RegisterOnSelected((evt) => CancelUpgrade());
        confirmUpgradeButton.RegisterOnSelected((evt) => ConfirmUpgrade());
        upgradedDeckPreviewButton.RegisterOnSelected((evt) => PreviewUpgradedDeck());
        companionUpgradeFocusables.Add(cancelUpgradeButton.AsFocusable());
        companionUpgradeFocusables.Add(confirmUpgradeButton.AsFocusable());
        companionUpgradeFocusables.Add(upgradedDeckPreviewButton.AsFocusable());

        VisualElement questionMark = uiDoc.rootVisualElement.Q<VisualElement>(name: "questionMark");
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
        uiDoc.rootVisualElement.Q<VisualElement>(name: "explainerText").AddToClassList("explainer-text-container-visible");
    }

    private void HideHelperText(PointerLeaveEvent evt) {
        uiDoc.rootVisualElement.Q<VisualElement>(name: "explainerText").RemoveFromClassList("explainer-text-container-visible");
    }

    private void ToggleAutoUpgrade(ClickEvent evt) {
        shopManager.SetAutoUpgrade((evt.target as Toggle).value);
    }

    private void SetupActiveSlots() {
        foreach (VisualElement child in activeContainer.hierarchy.Children())
        {
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
    private void SetupBenchSlots()
    {
        var children = benchContainer.hierarchy.Children().ToList();
        for (int i = 0; i < children.Count; i++) {
            VisualElement child = children[i];
            CompanionManagementSlotView slotView = new CompanionManagementSlotView(child,
                slotNotHighlightColor,
                slotHighlightColor,
                slotUnavailableColor);
            if (ProgressManager.Instance.IsFeatureEnabled(AscensionType.WORSE_BENCH) &&
                i >= ProgressManager.Instance.GetAscensionSO(AscensionType.WORSE_BENCH).
                ascensionModificationValues.GetValueOrDefault("numSlots", 3f)) {
                slotView.SetBlocked();
            }
            else {
                benchSlots.Add(slotView);
            }
        }
    }

    public VisualElement GetBenchSlotVE(int i)
    {
        if (i < 0 || i >= benchSlots.Count)
        {
            return null;
        }
        return benchSlots[i].root;
    }

    private void SetupMap(ShopManager shopManager)
    {
        mapContainer.Clear();
        mapContainer.Add(new MapView(shopManager.gameState.map.GetValue(), shopManager.gameState.currentEncounterIndex, EncounterType.Shop).mapContainer);
    }

    public void SetupUpgradeIncrements(bool shopFullyUpgraded = false) {
        upgradeIncrementContainer.Clear();
        if (shopFullyUpgraded) return;
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

    public void Clear()
    {
        ClearUnitManagement();
        shopGoodsArea.Clear();
        SetBlockedActiveSlotsIfNecessary(shopManager.gameState.companions.currentCompanionSlots);
    }

    // Handles setting up cards on first entry, see Reroll function for rerolling
    public void AddCardToShopView(CardInShopWithPrice card) {
        ShopItemView newCardItemView = new ShopItemView(this, card);

        shopGoodsArea.Add(newCardItemView.shopItemElement);

        cardItemToViewMap.Add(card, newCardItemView);

        FocusManager.Instance.RegisterFocusableTarget(newCardItemView.visualElementFocusable);
        disableOnCompanionDrag.Add(newCardItemView.visualElementFocusable);
    }

    // Handles removing cards when bought, see Reroll for rerolling
    public void RemoveCardFromShopView(CardInShopWithPrice card) {
        ShopItemView shopItemView = cardItemToViewMap[card];

        // TODO: Replace with sold out? Grey it out? Talk to Jasmine
        shopItemView.Disable();

        cardItemToViewMap.Remove(card);

        FocusManager.Instance.UnregisterFocusableTarget(shopItemView.visualElementFocusable);
        disableOnCompanionDrag.Remove(shopItemView.visualElementFocusable);
    }

    public void AnimateNewCompanionToSlot(CompanionInShopWithPrice companion, CompanionManagementSlotView companionManagementSlotView, bool isUpgrade = false, float delay = 0, Action onComplete = null)
    {
        ShopItemView shopItemView = companionItemToViewMap[companion];
        Vector2 startPoint = VisualElementUtils.GetCenterOfVisualElement(shopItemView.shopItemElement);
        Vector2 endPoint = VisualElementUtils.GetCenterOfVisualElement(companionManagementSlotView.root); // this is the root rather than the companionView (may have a slightly different width and height)

        shopItemView.Disable();

        companionItemToViewMap.Remove(companion);

        FocusManager.Instance.UnregisterFocusableTarget(shopItemView.visualElementFocusable);
        disableOnCompanionDrag.Remove(shopItemView.visualElementFocusable);

        CompanionMoveAnimation(startPoint, endPoint, shopItemView.shopItemElement, isUpgrade, delay, onComplete);

        CompanionView companionView = shopItemView.GetCompanionView();
        float widthRatio = CompanionViewOld.UNIT_MNGMT_CONTEXT.screenWidthPercent / CompanionViewOld.SHOP_CONTEXT.screenWidthPercent;
        float heightRatio = (CompanionViewOld.UNIT_MNGMT_CONTEXT.screenWidthPercent / CompanionViewOld.UNIT_MNGMT_CONTEXT.aspectRatio) / (CompanionViewOld.SHOP_CONTEXT.screenWidthPercent / CompanionViewOld.SHOP_CONTEXT.aspectRatio);
        float initialWidth = companionView.container.resolvedStyle.width;
        float initialHeight = companionView.container.resolvedStyle.height;

        // this animation will be a frame or two off from the move... could call this function from within the geometry changed callback
        LeanTween.value(1f, widthRatio, isUpgrade ? companionUpgradeTime : companionBuyTime)
            .setDelay(delay)
            .setEase(LeanTweenType.easeInSine)
            .setOnUpdate((float val) =>
            {
                companionView.container.style.width = initialWidth * val;
            });

        LeanTween.value(1f, heightRatio, isUpgrade ? companionUpgradeTime : companionBuyTime)
            .setDelay(delay)
            .setEase(LeanTweenType.easeInSine)
            .setOnUpdate((float val) =>
            {
                companionView.container.style.height = initialHeight * val;
            });
    }

    public void AnimateExistingCompanionToSlot(CompanionManagementSlotView startingSlotView, CompanionManagementSlotView endingSlotView, bool isUpgrade, float delay = 0, Action onComplete = null)
    {
        Vector2 startPoint = VisualElementUtils.GetCenterOfVisualElement(startingSlotView.root);
        Vector2 endPoint = VisualElementUtils.GetCenterOfVisualElement(endingSlotView.root);
        CompanionMoveAnimation(startPoint, endPoint, startingSlotView.root, isUpgrade, delay, onComplete); // which container do I move here :thinking:
    }


    private void CompanionMoveAnimation(Vector2 startPoint, Vector2 endPoint, VisualElement parentContainer, bool isUpgrade, float delay = 0, Action onComplete = null)
    {
        VisualElement tempContainer = new VisualElement();
        tempContainer.style.width = parentContainer.resolvedStyle.width;
        tempContainer.style.height = parentContainer.resolvedStyle.height;
        tempContainer.style.position = Position.Absolute;
        tempContainer.style.justifyContent = Justify.Center;
        tempContainer.style.alignItems = Align.Center;

        uiDoc.rootVisualElement.Add(tempContainer);
        VisualElement companionElement = parentContainer.Children().FirstOrDefault();
        tempContainer.style.left = startPoint.x - parentContainer.resolvedStyle.width / 2;
        tempContainer.style.top = startPoint.y - parentContainer.resolvedStyle.height / 2;

        // This on geometry changed callback ensures the tempcontainer is actually over the visualelement that is about to be moved when we start the animation
        // otherwise there is a chance that it is still at position 0,0 and there would be a brief flicker (very noticevable and jarring)
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if ((tempContainer.style.left == 0) || (tempContainer.style.right == 0)) return;
            // Unregister immediately when called
            tempContainer.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            // Start the animation
            StartCompanionMoveAnimation(companionElement, startPoint, endPoint, tempContainer, isUpgrade, delay, onComplete);
        }

        tempContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }


    private void StartCompanionMoveAnimation(VisualElement companionElement, Vector2 startPoint, Vector2 endPoint, VisualElement tempContainer, bool isUpgrade, float delay, Action onComplete = null)
    {
        tempContainer.Add(companionElement);
        // x
        LeanTween.value(startPoint.x, endPoint.x, isUpgrade ? companionUpgradeTime : companionBuyTime)
            .setDelay(delay)
            .setEase(LeanTweenType.easeInSine)
            .setOnUpdate((float val) =>
            {
                tempContainer.style.left = val - tempContainer.layout.width / 2;
            })
            .setOnComplete(() =>
            {
                tempContainer.style.left = endPoint.x - tempContainer.layout.width / 2;
                tempContainer.Clear();
                uiDoc.rootVisualElement.Remove(tempContainer);
                onComplete?.Invoke();
            });

        LeanTween.value(startPoint.y, endPoint.y, isUpgrade ? companionUpgradeTime : companionBuyTime)
            .setDelay(delay)
            .setEase(LeanTweenType.easeInSine)
            .setOnUpdate((float val) =>
            {
                tempContainer.style.top = val - tempContainer.layout.height / 2;
            })
            .setOnComplete(() =>
            {
                tempContainer.style.top = endPoint.y - tempContainer.layout.width / 2;
            });
    }

    public void CompanionUpgradeAnimation(CompanionManagementSlotView companionManagementSlotView, UpgradeInfo upgradeInfo)
    {
        FocusManager.Instance.DisableFocusableTarget(companionManagementSlotView.veFocusable);
        companionManagementSlotView.companionManagementView.SetUpdateAnimationPlaying(true);
        Companion companion = upgradeInfo.resultingCompanion;
        CompanionManagementView upgradedCompanion = new CompanionManagementView(
            companion,
            shopManager.encounterConstants.companionManagementViewTemplate,
            this);
        upgradedCompanion.container.style.visibility = Visibility.Hidden;
        upgradedCompanion.container.style.top = -100f;
        upgradedCompanion.SetUpdateAnimationPlaying(true);

        VisualElement visualElement = companionManagementSlotView.companionManagementView.container;

        LTSeq sequence = LeanTween.sequence();
        float height = -100f;
        MusicController.Instance.PlaySFX("event:/MX/MX_CompanionUpgradeStinger");
        // raise up
        LeanTween.value(0f, 1f, 1.5f)
            .setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float val) =>
            {
                visualElement.style.top = height * val;
            })
            .setOnComplete(() =>
            {
                shopManager.UpgradeSparkle(visualElement);
                shopManager.gameState.RemoveCompanionsFromTeam(new List<Companion> { companionManagementSlotView.companionManagementView.companion });
                companionManagementSlotView.Reset();
                // prolly throw a bunch of sparkles in here :shrug:
                companionManagementSlotView.InsertCompanion(upgradedCompanion);
                shopManager.gameState.AddCompanionToTeam(companion, upgradeInfo.onBench ? -1 : upgradeInfo.resultingSlotViewIndex);
                upgradedCompanion.container.style.visibility = Visibility.Visible;


                LeanTween.value(1f, 0f, .25f)
                    .setDelay(.5f)
                    .setEase(LeanTweenType.easeInQuint)
                    .setOnUpdate((float val) =>
                    {
                        upgradedCompanion.container.style.top = height * val;
                    })
                    .setOnComplete(() =>
                    {
                        FocusManager.Instance.EnableFocusableTarget(companionManagementSlotView.veFocusable);
                        upgradedCompanion.SetUpdateAnimationPlaying(false);
                        RebuildUnitManagement(shopManager.gameState.companions);
                        isBuyingDisabled = false;
                        canDragCompanions = true;
                        ScreenShakeManager.Instance.ShakeWithForce(1f);
                        MusicController.Instance.PlaySFX("event:/SFX/SFX_CompanionUpgradeThud");
                    });
            });
    }


    public CompanionManagementSlotView FindNextAvailableSlot()
    {
        for (int i = 0; i < activeSlots.Count; i++)
        {
            CompanionManagementSlotView companionManagementSlotView = activeSlots[i];
            if (!companionManagementSlotView.IsBlocked() && (companionManagementSlotView.companionManagementView == null)) return companionManagementSlotView;
        }
        for (int i = 0; i < benchSlots.Count; i++)
        {
            CompanionManagementSlotView companionManagementSlotView = benchSlots[i];
            if (!companionManagementSlotView.IsBlocked() && (companionManagementSlotView.companionManagementView == null)) return companionManagementSlotView;
        }
        Debug.LogError("Could not find an available slot");
        return null;
    }

    public CompanionManagementSlotView GetCompanionManagementSlotView(int index, bool onBench)
    {
        return onBench ? benchSlots[index] : activeSlots[index];
    }

    public CompanionManagementSlotView GetCompanionManagementSlotView(Companion companion)
    {
        for (int i = 0; i < activeSlots.Count; i++)
        {
            CompanionManagementSlotView companionManagementSlotView = activeSlots[i];
            if (companionManagementSlotView?.companionManagementView?.companion == companion) return companionManagementSlotView;
        }

        for (int i = 0; i < benchSlots.Count; i++)
        {
            CompanionManagementSlotView companionManagementSlotView = benchSlots[i];
            if (companionManagementSlotView?.companionManagementView?.companion == companion) return companionManagementSlotView;
        }
        Debug.LogError("Companion management slot view not found for companion");
        return null;
    }

    public void AnimateCardToCompanion(CardInShopWithPrice card, CompanionManagementView companionView)
    {
        ShopItemView shopItemView = cardItemToViewMap[card];
        Vector2 startPoint = VisualElementUtils.GetCenterOfVisualElement(shopItemView.shopItemElement);
        Vector2 endPoint = VisualElementUtils.GetCenterOfVisualElement(companionView.container);

        VisualElement tempContainer = new VisualElement();
        tempContainer.style.width = shopItemView.shopItemElement.resolvedStyle.width;
        tempContainer.style.height = shopItemView.shopItemElement.resolvedStyle.height;
        tempContainer.style.position = Position.Absolute;
        tempContainer.style.justifyContent = Justify.Center;
        tempContainer.style.alignItems = Align.Center;
        tempContainer.style.left = startPoint.x - shopItemView.shopItemElement.resolvedStyle.width / 2;
        tempContainer.style.top = startPoint.y - shopItemView.shopItemElement.resolvedStyle.height / 2;
        uiDoc.rootVisualElement.Add(tempContainer);

        // this ensures the tempcontainer is effectively positioned before transitioning the cardElement to tempcontainer
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if ((tempContainer.style.left == 0) || (tempContainer.style.right == 0)) return;
            // Unregister immediately when called
            tempContainer.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            VisualElement cardElement = shopItemView.shopItemElement.Children().FirstOrDefault();
            Debug.Log(cardElement);
            shopItemView.shopItemElement.Remove(cardElement);
            tempContainer.Add(cardElement);

            shopItemView.Disable();

            cardItemToViewMap.Remove(card);

            FocusManager.Instance.UnregisterFocusableTarget(shopItemView.visualElementFocusable);
            disableOnCompanionDrag.Remove(shopItemView.visualElementFocusable);

            int spinDirection = 0;
            if (startPoint.x < endPoint.x) spinDirection = 1;
            else spinDirection = -1;
            // Actually do the animation
            // x value
            LeanTween.value(startPoint.x, endPoint.x, cardBuyVFXTime)
                .setEase(cardBuyVFXHorizontal)
                .setOnUpdate((float val) =>
                {
                    tempContainer.style.left = val - tempContainer.layout.width / 2;
                })
                .setOnComplete(() =>
                {
                    shopManager.Sparkle(tempContainer);
                    tempContainer.Clear();
                    uiDoc.rootVisualElement.Remove(tempContainer);
                    CompanionScaleBumpAnimation(companionView.container);
                });
            // y value
            LeanTween.value(startPoint.y, endPoint.y, cardBuyVFXTime)
                .setEase(cardBuyVFXVertical)
                .setOnUpdate((float val) =>
                {
                    tempContainer.style.top = val - tempContainer.layout.height / 2;
                });
            // Rotation
            LeanTween.value(0f, spinDirection * 0.5f, cardBuyVFXTime)
                .setEase(cardBuyVFXRotation)
                .setOnUpdate((float value) =>
                {
                    float degreeRotation = 360f * value;
                    tempContainer.transform.rotation = Quaternion.AngleAxis(degreeRotation, Vector3.forward);
                });
            // Scale
            LeanTween.value(0.8f, 0.2f, cardBuyVFXTime)
                .setEase(cardBuyVFXScale)
                .setOnUpdate((float value) =>
                {
                    tempContainer.transform.scale = new Vector3(value, value, 0f);
                });
        }
        tempContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    public void CompanionScaleBumpAnimation(VisualElement container, float originalScale = 1f, float targetScale = 1.1f, float duration = .15f)
    {
        var sequence = LeanTween.sequence();

        sequence.append(LeanTween.value(originalScale, targetScale, duration * 0.4f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) => {
                container.transform.scale = new Vector3(val, val, 1f);
            }));

        sequence.append(LeanTween.value(targetScale, originalScale, duration * 0.6f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnUpdate((float val) => {
                container.transform.scale = new Vector3(val, val, 1f);
            }));
    }

    // Handles both the VFX and setting up the new area
    public void Reroll(List<CardInShopWithPrice> cards, List<CompanionInShopWithPrice> companions)
    {
        rerollButton.SetEnabled(false);
        VisualElement tempParentContainer = CreateTempContainer(shopGoodsArea.resolvedStyle.width, shopGoodsArea.resolvedStyle.height);
        tempParentContainer.style.overflow = Overflow.Hidden;
        uiDoc.rootVisualElement.Add(tempParentContainer);
        tempParentContainer.style.top = shopGoodsArea.worldBound.yMin;
        tempParentContainer.style.left = shopGoodsArea.worldBound.xMin;

        VisualElement oldItemsContainer = CreateTempContainer(shopGoodsArea.resolvedStyle.width, shopGoodsArea.resolvedStyle.height);
        VisualElement newItemsContainer = CreateTempContainer(shopGoodsArea.resolvedStyle.width, shopGoodsArea.resolvedStyle.height);

        // Manage cleanup of the maps
        Dictionary<CardInShopWithPrice, ShopItemView> tempCardItemToViewMap = new Dictionary<CardInShopWithPrice, ShopItemView>(cardItemToViewMap);
        foreach (KeyValuePair<CardInShopWithPrice, ShopItemView> pair in tempCardItemToViewMap)
        {
            FocusManager.Instance.UnregisterFocusableTarget(pair.Value.visualElementFocusable);
            disableOnCompanionDrag.Remove(pair.Value.visualElementFocusable);
            cardItemToViewMap.Remove(pair.Key);
        }

        Dictionary<CompanionInShopWithPrice, ShopItemView> tempCompanionItemToViewMap = new Dictionary<CompanionInShopWithPrice, ShopItemView>(companionItemToViewMap);
        foreach (KeyValuePair<CompanionInShopWithPrice, ShopItemView> pair in tempCompanionItemToViewMap)
        {
            FocusManager.Instance.UnregisterFocusableTarget(pair.Value.visualElementFocusable);
            disableOnCompanionDrag.Remove(pair.Value.visualElementFocusable);
            companionItemToViewMap.Remove(pair.Key);
        }

        // Move over the stuff from shop goods area to temp container
        // This can't happen in the above foreach loops because dictionaries aren't ordered
        List<VisualElement> tempChildren = new List<VisualElement>(shopGoodsArea.Children());
        foreach (VisualElement ve in tempChildren)
        {
            shopGoodsArea.Remove(ve);
            oldItemsContainer.Add(ve);
        }

        tempParentContainer.Add(oldItemsContainer);
        tempParentContainer.Add(newItemsContainer);
        newItemsContainer.style.top = 0;


        List<ShopItemView> newCardItems = new List<ShopItemView>();
        foreach (CardInShopWithPrice card in cards)
        {
            ShopItemView newCardItemView = new ShopItemView(this, card);
            newCardItemView.shopItemElement.style.top = -shopGoodsArea.resolvedStyle.height;
            newItemsContainer.Add(newCardItemView.shopItemElement);
            newCardItems.Add(newCardItemView);
        }

        List<ShopItemView> newCompanionItems = new List<ShopItemView>();
        foreach (CompanionInShopWithPrice companion in companions)
        {
            ShopItemView newCompanionItemView = new ShopItemView(this, companion, shopManager.encounterConstants.companionViewTemplate);
            newCompanionItemView.shopItemElement.style.top = -shopGoodsArea.resolvedStyle.height;
            newItemsContainer.Add(newCompanionItemView.shopItemElement);
            newCompanionItems.Add(newCompanionItemView);

        }

        int numItems = newItemsContainer.childCount;
        float delay = 0f;
        for (int i = 0; i < numItems; i++)
        {
            int index = i;
            List<VisualElement> oldItems = oldItemsContainer.Children().ToList();
            VisualElement newItem = newItemsContainer.Children().ToList()[i];
            // If you upgrade the shop and then reroll, there will be more new slots than old
            // slots. That's a corner case we deal with by checking the bounds.
            // It looks fine visually if we have the new item in the new slot roll in.
            if (i < oldItems.Count)
            {
                VisualElement oldItem = oldItems[i];
                // Make the old items move down below shop goods area
                LeanTween.value(0f, 1f, rerollVfxTime)
                    .setDelay(delay)
                    .setEase(oldItemsCurve)
                    .setOnUpdate((float value) =>
                    {
                        oldItem.style.top = 0f + (value * oldItemsContainer.resolvedStyle.height);
                    })
                    .setOnComplete(() =>
                    {
                        if (index != numItems - 1) return;
                        tempParentContainer.Remove(oldItemsContainer);
                        oldItemsContainer.Clear();
                        oldItemsContainer.RemoveFromHierarchy();
                    });
            }

            // Make the new items move down from above the shop goods area
            LeanTween.value(-1f, 0f, rerollVfxTime)
                .setDelay(oldToNewDelay + delay)
                .setEase(newItemsCurve)
                .setOnUpdate((float value) =>
                {
                    newItem.style.top = 0f + (value * newItem.resolvedStyle.height);
                })
                .setOnComplete(() =>
                {
                    if (index != numItems - 1) return;
                    foreach (ShopItemView newCardItem in newCardItems)
                    {
                        newItemsContainer.Remove(newCardItem.shopItemElement);
                        shopGoodsArea.Add(newCardItem.shopItemElement);
                        cardItemToViewMap.Add(newCardItem.cardInShop, newCardItem);
                        FocusManager.Instance.RegisterFocusableTarget(newCardItem.visualElementFocusable);
                        disableOnCompanionDrag.Add(newCardItem.visualElementFocusable);
                    }

                    foreach (ShopItemView newCompanionItem in newCompanionItems)
                    {
                        newItemsContainer.Remove(newCompanionItem.shopItemElement);
                        shopGoodsArea.Add(newCompanionItem.shopItemElement);
                        companionItemToViewMap.Add(newCompanionItem.companionInShop, newCompanionItem);
                        FocusManager.Instance.RegisterFocusableTarget(newCompanionItem.visualElementFocusable);
                        disableOnCompanionDrag.Add(newCompanionItem.visualElementFocusable);
                    }
                    tempParentContainer.Remove(newItemsContainer);
                    newItemsContainer.Clear();
                    newItemsContainer.RemoveFromHierarchy();
                    tempParentContainer.RemoveFromHierarchy();
                    rerollButton.SetEnabled(true);
                });
            delay += rerollTimeBetweenItems;
        }
    }

    private VisualElement CreateTempContainer(float width, float height)
    {
        VisualElement temp = new VisualElement();
        temp.style.width = width;
        temp.style.height = height;
        temp.style.position = Position.Absolute;
        temp.style.justifyContent = Justify.Center;
        temp.style.alignItems = Align.Center;
        temp.style.flexDirection = FlexDirection.Row;
        return temp;
    }

    // Handles setting up companions on first entry, see Reroll function for rerolling
    public void AddCompanionToShopView(CompanionInShopWithPrice companion) {
        ShopItemView newCompanionItemView = new ShopItemView(this, companion, shopManager.encounterConstants.companionViewTemplate);

        shopGoodsArea.Add(newCompanionItemView.shopItemElement);

        companionItemToViewMap.Add(companion, newCompanionItemView);

        FocusManager.Instance.RegisterFocusableTarget(newCompanionItemView.visualElementFocusable);
        disableOnCompanionDrag.Add(newCompanionItemView.visualElementFocusable);
    }

    // Handles removing cards when bought, see Reroll for rerolling
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
            if (!slotView.IsEmpty())
                slotView.companionManagementView.ResetToNeutral();
            slotView.Reset();
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (!slotView.IsEmpty())
                slotView.companionManagementView.ResetToNeutral();
            slotView.Reset();
        }
    }

    public void SetupActiveCompanions(List<Companion> companions) {
        for (int i = 0; i < companions.Count; i++) {
            CompanionManagementView companionView = new CompanionManagementView(
                    companions[i],
                    shopManager.encounterConstants.companionManagementViewTemplate,
                    this);
            activeSlots[i].InsertCompanion(companionView);
        }
    }

    public void SetupBenchCompanions(List<Companion> companions) {

        for (int i = 0; i < companions.Count; i++) {
            CompanionManagementView companionView = new CompanionManagementView(
                    companions[i],
                    shopManager.encounterConstants.companionManagementViewTemplate,
                    this);
            benchSlots[i].InsertCompanion(companionView);
        }
    }

    public void SetupCompanionManagementView(Companion companion, CompanionManagementSlotView companionManagementSlotView)
    {
        CompanionManagementView companionView = new CompanionManagementView(
            companion,
            shopManager.encounterConstants.companionManagementViewTemplate,
            this);
        companionManagementSlotView.InsertCompanion(companionView);
    }

    public void ShopItemOnClick(ShopItemView shopItemView)
    {
        if (isBuyingDisabled) return;
        if (shopItemView.companionInShop != null)
        {
            shopManager.ProcessCompanionBuyRequest(shopItemView, shopItemView.companionInShop);
        }
        else if (shopItemView.cardInShop != null)
        {
            shopManager.ProcessCardBuyRequestV2(shopItemView, shopItemView.cardInShop);
        }
    }

    public void RefreshCompanionViews()
    {
        foreach (CompanionManagementSlotView slotView in activeSlots) {
            if (slotView.companionManagementView != null)
            {
                slotView.companionManagementView.UpdateView();
            }
        }
        foreach (CompanionManagementSlotView slotView in benchSlots)
        {
            if (slotView.companionManagementView != null)
            {
                slotView.companionManagementView.UpdateView();
            }
        }
    }

    public void RerollButtonOnClick(ClickEvent evt)
    {
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
        shopManager.ProcessCompanionClicked(companionView);
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
        tempContainer.style.justifyContent = Justify.Center;
        tempContainer.style.alignItems = Align.Center;

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
                if (NumTakenSlots(benchSlots) < shopManager.AvailableBenchSlots) {
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

        if(evt != null) {
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
        moneyLabel.text = money.ToString() + "$";
    }

    public void NotEnoughMoney() {
        StartCoroutine(ShowGenericNotification("Not enough $!"));
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
            slotView.DisableSelling();
            if (!shopManager.IsApplicableCompanion(cardInShop, companion.companion)) {
                notApplicable.Add(companion);
                FocusManager.Instance.StashFocusableTarget(this.GetType().Name + "CardBuying", GetParentSlotViewForCompanion(companion).veFocusable);
            }
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (slotView.IsEmpty()) continue;
            CompanionManagementView companion = slotView.companionManagementView;
            slotView.DisableSelling();
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
            slotView.EnableSelling();
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (slotView.IsEmpty()) continue;
            slotView.companionManagementView.ResetApplicable();
            slotView.EnableSelling();
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
        FocusManager.Instance.EnableFocusableTarget(cancelCardRemovalButton.AsFocusable());
        StashNonCompanionViewFocusables(this.GetType().Name + "CardRemoval");
    }


    public void CardRemoved() {
        cardRemovalButton.SetEnabled(false);
        MusicController.Instance.PlaySFX("event:/SFX/SFX_CardRemoval");
        StartCoroutine(ShowGenericNotification("Card Removed!"));
        StopRemovingCard();
    }

    private void StopRemovingCard() {
        canDragCompanions = true;
        selectingCompanionVeil.style.visibility = Visibility.Hidden;
        selectingIndicatorForCardRemovalIndicator.style.visibility = Visibility.Hidden;
        FocusManager.Instance.DisableFocusableTarget(cancelCardRemovalButton.AsFocusable());
        FocusManager.Instance.UnstashFocusables(this.GetType().Name + "CardRemoval");
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
        background.style.left = upgradeButton.worldBound.xMax + 20;

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
        MultiDeckViewManager.Instance.ShowShopDeckView(false, companion, shopManager.gameState.companions.activeCompanions.Contains(companion) ? MultiDeckViewManager.TabType.Active : MultiDeckViewManager.TabType.Bench);
        /*
        if (cardSelectionViewPrefab != null)
        {
            GameObject cardSelectionViewGo = Instantiate(cardSelectionViewPrefab);
            CardSelectionView cardSelectionView = cardSelectionViewGo.GetComponent<CardSelectionView>();
            // Card Selection View stashes focusables on setup
            cardSelectionView.Setup(companion.getDeck().cards, companion, shopManager.encounterConstants.companionViewTemplate);
        }
        else
        { // Not sure why this else exists
            deckViewContentContainer.Clear();
            deckView.style.visibility = Visibility.Visible;

            foreach (Card card in companion.deck.cards)
            {
                CardView cardView = new CardView(card.cardType, companion.companionType, card.shopRarity, true);
                cardView.cardContainer.style.marginBottom = 10;
                cardView.cardContainer.style.marginLeft = 10;
                cardView.cardContainer.style.marginRight = 10;
                cardView.cardContainer.style.marginTop = 10;
                deckViewContentContainer.Add(cardView.cardContainer);
            }
        }*/
    }

    public void ShopDeckViewForCardRemoval(Companion companion) {
        if (cardSelectionViewPrefab != null) {
            GameObject cardSelectionViewGo = Instantiate(cardSelectionViewPrefab);
            CardSelectionView cardSelectionView = cardSelectionViewGo.GetComponent<CardSelectionView>();
            cardSelectionView.Setup(companion.getDeck().cards, "Select a Card For Removal!", 1, 1, companion, shopManager.encounterConstants.companionViewTemplate);
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

    public IEnumerator ShowGenericNotification(string text, float time = 1.25f)
    {
        if (waitAndHideMessageCoroutine != null)
        {
            StopCoroutine(waitAndHideMessageCoroutine);
        }

        genericMessageBox.style.visibility = Visibility.Visible;
        genericMessageBox.Q<Label>().text = text;
        waitAndHideMessageCoroutine = WaitAndHideMessageBox(time);
        StartCoroutine(waitAndHideMessageCoroutine);
        yield return null;
    }

    private IEnumerator WaitAndHideMessageBox(float time)
    {
        yield return new WaitForSeconds(time);
        genericMessageBox.style.visibility = Visibility.Hidden;
        waitAndHideMessageCoroutine = null;
    }
    public bool IsSellingCompanions()
    {
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

    public void ShowCompanionUpgradeMenu(List<Companion> companions, Companion upgradeCompanion)
    {
        currentUpgradeCompanion = upgradeCompanion;
        inUpgradeMenu = true;
        VisualElement upgradeMenuOuterContainer = uiDoc.rootVisualElement.Q<VisualElement>(name: "upgradeMenuOuterContainer");
        VisualElement companionUpgradeMenu = uiDoc.rootVisualElement.Q<VisualElement>(name: "upgradeMenuContainer");
        VisualElement initialCompanionContainer = uiDoc.rootVisualElement.Q<VisualElement>(name: "currentCompanions");
        VisualElement upgradeCompanionsContainer = uiDoc.rootVisualElement.Q<VisualElement>(name: "upgradedCompanion");
        initialCompanionContainer.Clear();
        upgradeCompanionsContainer.Clear();

        int delay = 500;
        int index;
        for (index = 0; index < companions.Count; index++)
        {
            var companion = companions[index];
            VisualElement companionContainer = new VisualElement();
            companionContainer.AddToClassList("victory-companion-container");
            companionContainer.AddToClassList("upgrade-menu-companion-invisible");

            CompanionTypeSO companionType = companion.companionType;
            CompanionView entityView = new CompanionView(companion, shopManager.encounterConstants.companionViewTemplate, index, CompanionView.COMPANION_UPGRADE_CONTEXT, null);
            VisualElement portraitContainer = entityView.container.Q(className: "companion-view-companion-image");
            portraitContainer.style.backgroundImage = new StyleBackground(companionType.sprite);
            companionContainer.Add(entityView.container);
            initialCompanionContainer.Add(companionContainer);

            companionUpgradeInstanceFocusables.Add((entityView, companionType, companionContainer));
            int displayIndex = index + 1;
            companionContainer.schedule.Execute(() =>
            {
                companionContainer.AddToClassList("upgrade-menu-compainion-1");
            }).StartingIn(delay);
            entityView.container.RegisterCallback<PointerEnterEvent>((evt) =>
            {
                tooltipController.DisplayTooltip(entityView.container, companion.companionType.GetTooltip(), TooltipContext.Shop);
            });
            entityView.container.RegisterCallback<PointerLeaveEvent>((evt) =>
            {
                tooltipController.DestroyTooltip(entityView.container);
            });
            delay += 250;
        }

        VisualElement upgradeCompanionContainer = new VisualElement();
        upgradeCompanionContainer.AddToClassList("victory-companion-container");
        upgradeCompanionContainer.AddToClassList("upgrade-menu-companion-invisible");
        CompanionTypeSO upgradeCompanionType = upgradeCompanion.companionType;
        CompanionView upgradeEntityView = new CompanionView(upgradeCompanion, shopManager.encounterConstants.companionViewTemplate, index + 1, CompanionView.COMPANION_UPGRADE_CONTEXT, null);
        //entityView.entityContainer.AddToClassList("compendium-item-container");
        VisualElement upgradePortraitContainer = upgradeEntityView.container.Q(className: "companion-view-companion-image");
        upgradePortraitContainer.style.backgroundImage = new StyleBackground(upgradeCompanionType.sprite);
        upgradeCompanionContainer.Add(upgradeEntityView.container);
        upgradeCompanionContainer.schedule.Execute(() =>
        {
            upgradeCompanionContainer.AddToClassList("upgrade-menu-compainion-1");
        }).StartingIn(delay);
        upgradeCompanionContainer.schedule.Execute(() =>
        {
            // update hoverable pos after animation
            // UIDocumentHoverableInstantiator.Instance.UpdateHoverablesPosition();
        }).StartingIn(delay + 1000); // GET RACE CONDITIONED LOSER
        upgradeEntityView.container.RegisterCallback<PointerEnterEvent>((evt) =>
        {
            UpgradeViewCompanionOnPointerEnter(upgradeEntityView, upgradeCompanionType.GetTooltip(), false);
        });
        upgradeEntityView.container.RegisterCallback<PointerLeaveEvent>((evt) =>
        {
            UpgradeViewcompanionOnPointerLeave(upgradeEntityView);
        });
        companionUpgradeInstanceFocusables.Add((upgradeEntityView, upgradeCompanionType, upgradeCompanionContainer));
        upgradeCompanionsContainer.Add(upgradeCompanionContainer);
        companionUpgradeMenu.AddToClassList("upgrade-menu-container-visible");
        upgradeMenuOuterContainer.AddToClassList("upgrade-menu-outer-container-visible");
        upgradeMenuOuterContainer.pickingMode = PickingMode.Position;
        FocusManager.Instance.StashFocusables(this.GetType().Name + UPGRADE_MENU);
        companionUpgradeFocusables.ForEach((focusable) => FocusManager.Instance.EnableFocusableTarget(focusable));
        companionUpgradeInstanceFocusables.ForEach((focusable) =>
        {
            VisualElementFocusable VEFocusable = focusable.Item1.container.AsFocusable();
            FocusManager.Instance.RegisterFocusableTarget(VEFocusable);
            FocusManager.Instance.EnableFocusableTarget(VEFocusable);
            VEFocusable.additionalFocusAction += () => UpgradeViewCompanionOnPointerEnter(focusable.Item1, focusable.Item2.GetTooltip(), false);
            VEFocusable.additionalUnfocusAction += () => UpgradeViewcompanionOnPointerLeave(focusable.Item1);
        });
    }

    private void UpgradeViewCompanionOnPointerEnter(CompanionView companionView, TooltipViewModel tooltipViewModel, bool forCompanionManagementView)
    {
        tooltipController.DisplayTooltip(companionView.container, tooltipViewModel, TooltipContext.CompanionManagementView);
        companionView.SetSelectionIndicatorVisibility(true);
    }

    private void UpgradeViewcompanionOnPointerLeave(CompanionView companionView)
    {
        tooltipController.DestroyTooltip(companionView.container);
        companionView.SetSelectionIndicatorVisibility(false);
    }

    private void CancelUpgrade()
    {
        if (!inUpgradeMenu) return;
        inUpgradeMenu = false;
        VisualElement upgradeMenuOuterContainer = uiDoc.rootVisualElement.Q<VisualElement>(name: "upgradeMenuOuterContainer");
        VisualElement companionUpgradeMenu = uiDoc.rootVisualElement.Q<VisualElement>(name: "upgradeMenuContainer");
        companionUpgradeMenu.RemoveFromClassList("upgrade-menu-container-visible");
        upgradeMenuOuterContainer.RemoveFromClassList("upgrade-menu-outer-container-visible");
        upgradeMenuOuterContainer.pickingMode = PickingMode.Ignore;
        shopManager.CancelUpgradePurchase();
        FocusManager.Instance.UnstashFocusables(this.GetType().Name + UPGRADE_MENU);
        companionUpgradeFocusables.ForEach((focusable) => FocusManager.Instance.DisableFocusableTarget(focusable));
        companionUpgradeInstanceFocusables.ForEach((focusable) =>
        {
            VisualElementFocusable VEFocusable = focusable.Item1.container.AsFocusable();
            FocusManager.Instance.DisableFocusableTarget(VEFocusable);
            FocusManager.Instance.UnregisterFocusableTarget(VEFocusable);
        });
        companionUpgradeInstanceFocusables.Clear();
    }

    private void ConfirmUpgrade()
    {
        if (!inUpgradeMenu) return;
        inUpgradeMenu = false;

        VisualElement upgradeMenuOuterContainer = uiDoc.rootVisualElement.Q<VisualElement>(name: "upgradeMenuOuterContainer");
        VisualElement companionUpgradeMenu = uiDoc.rootVisualElement.Q<VisualElement>(name: "upgradeMenuContainer");
        companionUpgradeMenu.RemoveFromClassList("upgrade-menu-container-visible");
        upgradeMenuOuterContainer.RemoveFromClassList("upgrade-menu-outer-container-visible");
        upgradeMenuOuterContainer.pickingMode = PickingMode.Ignore;
        shopManager.ConfirmUpgradePurchase();
        FocusManager.Instance.UnstashFocusables(this.GetType().Name + UPGRADE_MENU);
        companionUpgradeFocusables.ForEach((focusable) => FocusManager.Instance.DisableFocusableTarget(focusable));
        companionUpgradeInstanceFocusables.ForEach((focusable) =>
        {
            VisualElementFocusable VEFocusable = focusable.Item1.container.AsFocusable();
            FocusManager.Instance.DisableFocusableTarget(VEFocusable);
            FocusManager.Instance.UnregisterFocusableTarget(VEFocusable);
        });
        companionUpgradeInstanceFocusables.Clear();
    }

    public void DisplayTooltip(VisualElement element, TooltipViewModel tooltipViewModel, TooltipContext context) {
        tooltipController.DisplayTooltip(element, tooltipViewModel, context);
    }

    public void DestroyTooltip(VisualElement element) {
        tooltipController.DestroyTooltip(element);
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        // Just need the IControlsReceiver for the SwappedControlMethod
        if (action == GFGInputAction.OPEN_MULTI_DECK_VIEW)
        {
            MultiDeckViewManager.Instance.ShowShopDeckView();
        }
        return;
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        if (isDraggingCompanion) {
            FocusManager.Instance.UnstashFocusables(this.GetType().Name + COMPANION_MANAGEMENT);
            // abort dragging companion in a *nice* way
            VisualElement tempContainer = companionBeingDragged.container.parent;
            originalSlot.InsertCompanion(companionBeingDragged);
            originalSlot.SetNotHighlighted();
            uiDoc.rootVisualElement.Remove(tempContainer);
            isDraggingCompanion = false;
            companionBeingDragged = null;
            originalSlot = null;
        }

        foreach (CompanionManagementSlotView slotView in activeSlots) {
            if (slotView.IsEmpty()) continue;
            slotView.companionManagementView.ResetToNeutral();
        }

        foreach (CompanionManagementSlotView slotView in benchSlots) {
            if (slotView.IsEmpty()) continue;
            slotView.companionManagementView.ResetToNeutral();
        }
    }

    public void DisplayCards(CompanionTypeSO companion)
    {
        Companion companionToShow = new Companion(companion);
        for (int i = 0; i < shopManager.GetShopLevel().numLessCardsInStartingDeck; i++) {
            companionToShow.deck.PurgeStarterDeckCard(ShopManager.Instance.gameState.baseShopData.baseCardsToRemoveOnUpgrade);
        }
        MultiDeckViewManager.Instance.ShowShopDeckView(true, companionToShow, MultiDeckViewManager.TabType.ForPurchase);
    }
}
