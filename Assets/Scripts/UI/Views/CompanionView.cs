using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionView : IUIEventReceiver
{
    // Currently the frame itself has a 4:5 aspect ratio, but with the extra
    // space the entire companion view template has a 5:4 aspect ratio
    public static CompanionViewContext COMBAT_CONTEXT =
        new CompanionViewContext(true, false, true, true, false, false, false, false, 1.25f, 0.175f);

    public static CompanionViewContext COMBAT_CONTEXT_VISUAL_ONLY =
        new CompanionViewContext(false, false, false, false, false, false, false, false, 1.25f, 0.175f, purelyVisual:true);

    public static CompanionViewContext SHOP_CONTEXT =
        new CompanionViewContext(false, true, false, false, true, false, false, true, 0.8f, 0.15f * .75f);

    public static CompanionViewContext UNIT_MNGMT_CONTEXT =
        new CompanionViewContext(false, false, false, false, true, true, true, false, 1.1f, 0.15f * .6f);

    public static CompanionViewContext CARD_SELECTION_CONTEXT =
        new CompanionViewContext(false, false, true, true, false, false, false, false, 1.25f, 0.2f);

    public static CompanionViewContext COMPENDIUM_CONTEXT =
        new CompanionViewContext(false, false, false, true, false, false, false, false, 1.25f, 0.2f);

    public static CompanionViewContext COMPANION_UPGRADE_CONTEXT =
        new CompanionViewContext(false, false, false, false, true, false, false, true, 0.8f, 0.15f, true);

    public VisualElement container;
    public VisualElementFocusable focusable;

    // private IUIEntity entity;
    private ICompanionViewDelegate viewDelegate;
    private VisualTreeAsset template;
    private int index;
    private CompanionViewContext context;
    private CompanionInstance companionInstance = null;
    private Companion companion = null;
    private CombatInstance combatInstance = null;

    private VisualElement parentContainer;
    private VisualElement statusVertical;
    private VisualElement statusContainer;
    private VisualElement extraSpace;
    private VisualElement solidBackground;
    private VisualElement imageElement;
    private VisualElement lowerHoverDetector;
    private VisualElement drawDiscardContainer;
    private VisualElement viewDeckContainer;
    private IconButton drawPileButton;
    private IconButton discardPileButton;
    private IconButton viewDeckButton;
    private Label primaryNameLabel;
    private Label secondaryNameLabel;
    private Label blockLabel;
    private Label healthLabel;
    private Label maxHealthLabel;
    private VisualElement maxHealthContainer;
    private VisualElement selectedIndicator;
    private VisualElement rarityMediumIndicator;
    private VisualElement rarityHighIndicator;
    private VisualElement levelOneIndicator;
    private VisualElement levelTwoIndicator;
    private VisualElement levelThreeIndicator;

    private List<VisualElement> pickingModePositionList = new List<VisualElement>();
    private List<VisualElement> elementsKeepingHiddenContainerVisible = new List<VisualElement>();
    private bool isDead = false;
    private VisualElement containerThatHoverIndicatorShows;
    private Coroutine maxHealthIndicatorCoroutine;
    private bool isTweening = false;

    public CompanionView(
            Companion companion,
            VisualTreeAsset template,
            int index,
            CompanionViewContext context,
            ICompanionViewDelegate viewDelegate,
            CompanionInstance companionInstance = null) {
        this.companionInstance = companionInstance;
        this.companion = companion;
        this.viewDelegate = viewDelegate;
        this.template = template;
        this.index = index;
        this.context = context;

        if (this.companionInstance != null) {
            this.combatInstance = this.companionInstance.combatInstance;
        }

        SetupCompanionView();

        if (this.combatInstance && !context.purelyVisual)
        {
            combatInstance.onDamageHandler += DamageScaleBump;
            combatInstance.onDeathHandler += OnDeathHandler;
            combatInstance.SetVisualElement(this.container);
        }
    }

    private void SetupCompanionView()
    {
        VisualElement companionRoot = this.template.CloneTree();

        this.parentContainer = companionRoot.Q<VisualElement>("companion-view-parent-container");
        this.statusContainer = companionRoot.Q<VisualElement>("companion-view-status-container");
        this.statusVertical = companionRoot.Q<VisualElement>("companion-view-status-vertical");
        this.extraSpace = companionRoot.Q<VisualElement>("companion-view-extra-space");
        this.solidBackground = companionRoot.Q<VisualElement>("companion-view-solid-background");
        this.imageElement = companionRoot.Q<VisualElement>("companion-view-companion-image");
        this.primaryNameLabel = companionRoot.Q<Label>("companion-view-primary-name-label");
        this.secondaryNameLabel = companionRoot.Q<Label>("companion-view-secondary-name-label");
        this.blockLabel = companionRoot.Q<Label>("companion-view-block-value-label");
        this.healthLabel = companionRoot.Q<Label>("companion-view-health-value-label");
        this.maxHealthLabel = companionRoot.Q<Label>("companion-view-max-health-label");
        this.maxHealthContainer = companionRoot.Q<VisualElement>("companion-view-max-health-indicator");
        this.lowerHoverDetector = companionRoot.Q<VisualElement>("companion-view-lower-hover-detector");
        this.drawDiscardContainer = companionRoot.Q<VisualElement>("companion-view-draw-discard-container");
        this.viewDeckContainer = companionRoot.Q<VisualElement>("companion-view-view-deck-container");
        this.drawPileButton = companionRoot.Q<IconButton>("companion-view-draw-pile-button");
        this.discardPileButton = companionRoot.Q<IconButton>("companion-view-discard-pile-button");
        this.viewDeckButton = companionRoot.Q<IconButton>("companion-view-view-deck-button");
        this.selectedIndicator = companionRoot.Q<VisualElement>("companion-view-selected-indicator");
        this.rarityMediumIndicator = companionRoot.Q<VisualElement>("companion-view-rarity-medium-indicator");
        this.rarityHighIndicator = companionRoot.Q<VisualElement>("companion-view-rarity-high-indicator");
        this.levelOneIndicator = companionRoot.Q<VisualElement>("companion-view-level-1-indicator");
        this.levelTwoIndicator = companionRoot.Q<VisualElement>("companion-view-level-2-indicator");
        this.levelThreeIndicator = companionRoot.Q<VisualElement>("companion-view-level-3-indicator");

        // Moving past the random VisualElement parent CloneTree() creates
        this.container = companionRoot.Children().First();
        this.container.name = container.name + this.index;
        this.pickingModePositionList.Add(container);
        SetupMainContainer();
        SetupBackground();
        SetupCompanionSprite();
        SetupName();
        SetupBlockAndHealth();
        SetupStatusIndicators();
        SetupLevelIndicator();
        SetupRarityIndicator();

        if (this.context.setupDrawDiscardButtons || this.context.setupViewDeckButton) SetupHoverDetector();
        if (this.context.setupDrawDiscardButtons) SetupDrawDiscardContainer();
        if (this.context.setupViewDeckButton) SetupViewDeckContainer();

        UpdateWidthAndHeight(container);
    }

    private void SetupLevelIndicator() {
        switch (this.companion.companionType.level) {
            case CompanionLevel.LevelThree:
                this.levelThreeIndicator.style.display = DisplayStyle.Flex;
            break;

            case CompanionLevel.LevelTwo:
                this.levelTwoIndicator.style.display = DisplayStyle.Flex;
            break;

            case CompanionLevel.LevelOne:
            default:
                this.levelOneIndicator.style.display = DisplayStyle.Flex;
            break;
        }
    }

    private void SetupRarityIndicator() {
        switch (this.companion.companionType.rarity) {
            case CompanionRarity.RARE:
                this.rarityHighIndicator.style.display = DisplayStyle.Flex;
            break;

            case CompanionRarity.UNCOMMON:
                this.rarityMediumIndicator.style.display = DisplayStyle.Flex;
            break;

            case CompanionRarity.COMMON:
            default:
                // The companion frame itself has the common gem built into the asset
            break;
        }
    }

    private void SetupBackground() {
        Color color = this.companion.companionType.pack.packColor;
        this.solidBackground.style.backgroundColor = color;
    }

    private void SetupBlockAndHealth() {
        if (this.companionInstance == null) {
            this.healthLabel.text = this.companion.GetCurrentHealth().ToString();
            this.maxHealthLabel.text = this.companion.GetCombatStats().getMaxHealth().ToString();
            this.blockLabel.style.visibility = Visibility.Hidden;
        } else {
            this.healthLabel.text = this.combatInstance.combatStats.currentHealth.ToString();
            this.maxHealthLabel.text = this.combatInstance.combatStats.maxHealth.ToString();
            this.blockLabel.text = this.combatInstance.GetStatus(StatusEffectType.Defended).ToString();
        }

        if (this.context.enableMaxHealthIndicator) {
            this.healthLabel.RegisterCallback<PointerEnterEvent>((evt) => {
                this.maxHealthContainer.style.visibility = Visibility.Visible;
            });

            this.healthLabel.RegisterCallback<PointerLeaveEvent>((evt) => {
                this.maxHealthContainer.style.visibility = Visibility.Hidden;
            });
        }

        this.pickingModePositionList.Add(this.healthLabel);
    }

    private void SetupStatusIndicators()
    {
        this.statusContainer.Clear();

        if (this.combatInstance == null) return;

        foreach (KeyValuePair<StatusEffectType, int> kvp in combatInstance.GetDisplayedStatusEffects())
        {
            // Block is displayed on the companion frame now :)
            if (kvp.Key == StatusEffectType.Defended) continue;
            this.statusContainer.Add(CreateStatusIndicator(viewDelegate.GetStatusEffectSprite(kvp.Key), kvp.Value.ToString()));
        }

        List<DisplayedCacheValue> cacheValues = combatInstance.GetDisplayedCacheValues();
        foreach (DisplayedCacheValue cacheValue in cacheValues)
        {
            this.statusContainer.Add(CreateStatusIndicator(cacheValue.sprite, cacheValue.value.ToString()));
        }

        List<(PowerSO, int)> activePowers = combatInstance.GetPowersWithStackCounts();
        foreach ((PowerSO, int) p in activePowers)
        {
            string val = p.Item1.stackable ?  p.Item2.ToString() : "";
            this.statusContainer.Add(CreateStatusIndicator(p.Item1.displaySprite, val));
        }
    }

    private VisualElement CreateStatusIndicator(Sprite icon, string textValue) {
        VisualElement statusIndicator = new VisualElement();
        statusIndicator.AddToClassList("companion-view-status-indicator");

        Label statusLabel = new Label();
        statusLabel.AddToClassList("companion-view-status-indicator-label");
        statusLabel.text = textValue;

        VisualElement statusIcon = new VisualElement();
        statusIcon.AddToClassList("companion-view-status-indicator-icon");
        statusIcon.style.backgroundImage = new StyleBackground(icon);

        statusIndicator.Add(statusLabel);
        statusIndicator.Add(statusIcon);

        return statusIndicator;
    }

    private void SetupMainContainer() {
        this.container.RegisterOnSelected(() => ContainerPointerClick(null), false);
        this.container.RegisterCallback<PointerEnterEvent>(ContainerPointerEnter);
        this.container.RegisterCallback<PointerLeaveEvent>(ContainerPointerLeave);

        this.focusable = this.container.AsFocusable();
        this.focusable.additionalFocusAction += () => ContainerPointerEnter(null);
        this.focusable.additionalUnfocusAction += () => ContainerPointerLeave(null);

        this.focusable.SetInputAction(GFGInputAction.VIEW_DECK, () => {
            if (this.context.setupViewDeckButton) ViewDeckButtonOnClick(null);
            else if (this.context.setupDrawDiscardButtons) DrawPileButtonOnClick(null);
            else return;
        });
        this.focusable.SetInputAction(GFGInputAction.VIEW_DISCARD, () => DiscardPileButtonOnClick(null));

        if (this.context.trimToJustMainBody) {
            this.statusVertical.style.display = DisplayStyle.None;
            this.extraSpace.style.display = DisplayStyle.None;
        }
    }

    public void SetSelectionIndicatorVisibility(bool visible)
    {
        this.selectedIndicator.visible = visible;
    }

    private void ContainerPointerClick(ClickEvent evt) {
        try {
            if (this.companionInstance == null) return;
            Targetable targetable = this.companionInstance.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerClickUI(evt);
        } catch (Exception e) {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," +
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }

    private void ContainerPointerEnter(PointerEnterEvent evt) {
        if (isDead) return;

        // Shop does it's own thing for hovering over companions
        if (this.context.enableSelectedIndicator)
            this.selectedIndicator.style.visibility = Visibility.Visible;

        // Pointer enter came from focus
        // Setting this up for supporting showing the max health indicator when
        // the companion is focused for a couple seconds
        if (evt == null && this.context.enableMaxHealthIndicator) {
            this.maxHealthIndicatorCoroutine = CoroutineRunner.Instance.Run(ShowMaxHealthIndicatorAfterDelay());
        }

        try {
            if (this.context.preventDefaultDeckViewButton || this.companionInstance == null) return;
            Targetable targetable = this.companionInstance.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerEnterUI(evt);
        } catch (Exception e) {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," +
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }

    private void ContainerPointerLeave(PointerLeaveEvent evt)
    {
        if (isDead) return;

        this.selectedIndicator.style.visibility = Visibility.Hidden;

        // Setting this up for supporting showing the max health indicator when
        // the companion is focused for a couple seconds
        if (this.maxHealthIndicatorCoroutine != null) {
            CoroutineRunner.Instance.Stop(this.maxHealthIndicatorCoroutine);
        }
        this.maxHealthContainer.style.visibility = Visibility.Hidden;

        try
        {
            if (this.context.preventDefaultDeckViewButton || this.companionInstance == null) return;

            Targetable targetable = this.companionInstance.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerLeaveUI(evt);
        }
        catch (Exception e)
        {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," +
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }

    private void SetupViewDeckContainer() {
        if (this.context.preventDefaultDeckViewButton) return;
        this.viewDeckContainer.RegisterCallback<PointerEnterEvent>(HiddenContainerPointerEnter);
        this.viewDeckContainer.RegisterCallback<PointerLeaveEvent>(HiddenContainerPointerLeave);
        pickingModePositionList.Add(this.viewDeckContainer);

        this.viewDeckButton.SetIcon(
                GFGInputAction.VIEW_DECK,
                ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.VIEW_DECK));
        ControlsManager.Instance.RegisterIconChanger(this.viewDeckButton);

        this.viewDeckButton.RegisterCallback<ClickEvent>(ViewDeckButtonOnClick);

        this.containerThatHoverIndicatorShows = this.viewDeckContainer;

        this.pickingModePositionList.Add(this.viewDeckButton);
    }

    private void ViewDeckButtonOnClick(ClickEvent evt) {
        if (evt != null) evt.StopPropagation();
        // Luke needs to fix this because it's bad but I'm in a rush
        viewDelegate.ViewDeck(DeckViewType.EntireDeck, companion);
    }

    private void SetupDrawDiscardContainer() {
        this.drawDiscardContainer.RegisterCallback<PointerEnterEvent>(HiddenContainerPointerEnter);
        this.drawDiscardContainer.RegisterCallback<PointerLeaveEvent>(HiddenContainerPointerLeave);
        pickingModePositionList.Add(this.drawDiscardContainer);

        this.drawPileButton.SetIcon(
                GFGInputAction.VIEW_DECK,
                ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.VIEW_DECK));
        this.discardPileButton.SetIcon(
                GFGInputAction.VIEW_DISCARD,
                ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.VIEW_DISCARD));
        ControlsManager.Instance.RegisterIconChanger(this.drawPileButton);
        ControlsManager.Instance.RegisterIconChanger(this.discardPileButton);

        this.drawPileButton.RegisterCallback<ClickEvent>(DrawPileButtonOnClick);
        this.discardPileButton.RegisterCallback<ClickEvent>(DiscardPileButtonOnClick);

        this.containerThatHoverIndicatorShows = this.drawDiscardContainer;

        this.pickingModePositionList.Add(this.drawPileButton);
        this.pickingModePositionList.Add(this.discardPileButton);
    }

    private void DrawPileButtonOnClick(ClickEvent evt)
    {
        if (evt != null) evt.StopPropagation();
        Debug.Log("Draw button clicked");
        if (this.companionInstance != null)
        {
            viewDelegate.ViewDeck(DeckViewType.Draw, null, this.companionInstance);
        }
    }

    private void DiscardPileButtonOnClick(ClickEvent evt) {
        if (evt != null) evt.StopPropagation();
        Debug.Log("Discard button clicked");
        if (this.companionInstance != null)
        {
            viewDelegate.ViewDeck(DeckViewType.Discard, null, this.companionInstance);
        }
    }

    private void HiddenContainerPointerEnter(PointerEnterEvent evt) {
        if (isDead) return;
        // If we're entering the element, it's already visible due to the hover detector
        elementsKeepingHiddenContainerVisible.Add(evt.currentTarget as VisualElement);
    }

    private void HiddenContainerPointerLeave(PointerLeaveEvent evt) {
        elementsKeepingHiddenContainerVisible.Remove(evt.currentTarget as VisualElement);
        CoroutineRunner.Instance.Run(HideContainerAtEndOfFrame());
    }

    private void SetupHoverDetector() {
        this.pickingModePositionList.Add(this.lowerHoverDetector);
        this.lowerHoverDetector.RegisterCallback<PointerEnterEvent>(HoverDetectorPointerEnter);
        this.lowerHoverDetector.RegisterCallback<PointerLeaveEvent>(HoverDetectorPointerLeave);

        this.focusable.additionalFocusAction += () => HoverDetectorPointerEnter(this.container.CreateFakePointerEnterEvent());
        this.focusable.additionalUnfocusAction += () => HoverDetectorPointerLeave(this.container.CreateFakePointerLeaveEvent());
    }

    public void HoverDetectorPointerEnter(PointerEnterEvent evt) {
        if (this.isDead) return;
        // This null check exists because ShopItemView will call this with a null event
        // if a shop item is hovered with non mouse controls
        if (this.context.preventDefaultDeckViewButton) return;
        if (evt != null) elementsKeepingHiddenContainerVisible.Add(evt.currentTarget as VisualElement);
        // this.containerThatHoverIndicatorShows.style.visibility = Visibility.Visible;

        this.containerThatHoverIndicatorShows.style.display = DisplayStyle.Flex;
    }

    public void HoverDetectorPointerLeave(PointerLeaveEvent evt) {
        // This null check exists because ShopItemView will call this with a null event
        // if a shop item is hovered with non mouse controls
        if (this.context.preventDefaultDeckViewButton) return;
        if (evt != null) elementsKeepingHiddenContainerVisible.Remove(evt.currentTarget as VisualElement);
        CoroutineRunner.Instance.Run(HideContainerAtEndOfFrame());
    }

    private IEnumerator HideContainerAtEndOfFrame() {
        yield return new WaitForEndOfFrame();
        if (elementsKeepingHiddenContainerVisible.Count == 0) {
            this.containerThatHoverIndicatorShows.style.display = DisplayStyle.None;
        }
    }

    private void SetupCompanionSprite() {
        Sprite sprite = this.companion.getSprite();
        this.imageElement.style.backgroundImage = new StyleBackground(sprite);
        if (this.context.makeSpriteFillSpace) {
            this.imageElement.AddToClassList("companion-view-companion-image-fill-space");
        }
    }

    private void SetupName() {
        if (this.context.disableNametags) {
            this.primaryNameLabel.style.display = DisplayStyle.None;
            this.secondaryNameLabel.style.display = DisplayStyle.None;
            return;
        } else if (this.context.smallNametag) {
            this.primaryNameLabel.AddToClassList("companion-view-primary-name-label-small");
        }
        this.primaryNameLabel.text = this.companion.GetName();
        this.secondaryNameLabel.text = ""; // TODO: Do this lmao
    }

    public void ScaleView(float scale) {
        UpdateWidthAndHeight(this.container, scale);
    }

    public void UpdateWidthAndHeight(VisualElement root, float scale = 1f) {
        Tuple<int, int> entityWidthHeight = GetWidthAndHeight(scale);
        root.style.width = entityWidthHeight.Item1;
        root.style.height = entityWidthHeight.Item2;
    }

    private Tuple<int, int> GetWidthAndHeight(float scale) {
        int width = (int)(Screen.width * this.context.screenWidthPercent * scale);
        int height = (int)(width / this.context.aspectRatio);

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * this.context.screenWidthPercent * scale);
        height = (int)(width / this.context.aspectRatio);
        #endif

        return new Tuple<int, int>(width, height);
    }

    public void SetPickingModes(bool enable) {
        foreach (VisualElement ve in pickingModePositionList) {
            ve.pickingMode = enable ? PickingMode.Position : PickingMode.Ignore;
        }
    }

    private void DamageScaleBump(int scale) {
        if (scale == 0 || this.isTweening) return; // this could mean the damage didn't go through the block or that the companion died while taking damage

        float duration = 0.125f;  // Total duration for the scale animation
        float minScale = .8f; // (float)Math.Min(.75, .9 - scale / 500);  // scale bump increases in intensity if entity takes more damage (haven't extensively tested this)

        Vector2 originalElementScale = new Vector2(
            this.container.style.scale.value.value.x,
            this.container.style.scale.value.value.y
        );

        LeanTween.value(1f, minScale, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1) // inverse tween is called when this tween completes. On complete below is called after both tweens complete
            .setOnUpdate((float currentScale) =>
            {
                this.container.style.scale = new StyleScale(new Scale(originalElementScale * currentScale));
            })
            .setOnStart(() =>
            {
                this.isTweening = true;
            })
            .setOnComplete(() =>
            {
                this.isTweening = false;
                this.container.style.scale = new StyleScale(new Scale(originalElementScale));
            });
    }

    public void UpdateView() {
        SetupBlockAndHealth();
        SetupStatusIndicators();
    }

    private IEnumerator OnDeathHandler(CombatInstance killer) {
        FocusManager.Instance.UnregisterFocusableTarget(this.focusable);
        isDead = true;
        yield return null;
    }

    private IEnumerator ShowMaxHealthIndicatorAfterDelay()
    {
        yield return new WaitForSeconds(0.8f);
        this.maxHealthContainer.style.visibility = Visibility.Visible;
        this.maxHealthIndicatorCoroutine = null;
    }

    public IEnumerator AbilityActivatedVFX() {
        CompanionView clonedCompanionView = new CompanionView(this.companion, this.template, 0, COMBAT_CONTEXT_VISUAL_ONLY, this.viewDelegate, this.companionInstance);
        yield return EntityAbilityInstance.GenericAbilityTriggeredVFX(this.container, clonedCompanionView.container);
    }
}

public class CompanionViewContext {
    public bool setupDrawDiscardButtons;
    public bool setupViewDeckButton;
    public bool enableMaxHealthIndicator;
    public bool enableSelectedIndicator;
    public bool trimToJustMainBody;
    public bool makeSpriteFillSpace;
    public bool disableNametags;
    public bool smallNametag;
    public float aspectRatio;
    public float screenWidthPercent;
    public bool preventDefaultDeckViewButton;
    public bool purelyVisual;

    public CompanionViewContext(
            bool setupDrawDiscardButtons,
            bool setupViewDeckButton,
            bool enableMaxHealthIndicator,
            bool enableSelectedIndicator,
            bool trimToJustMainBody,
            bool makeSpriteFillSpace,
            bool disableNametags,
            bool smallNametag,
            float aspectRatio,
            float screenWidthPercent,
            bool preventDefaultDeckViewButton = false,
            bool purelyVisual = false)
    {
        this.setupDrawDiscardButtons = setupDrawDiscardButtons;
        this.setupViewDeckButton = setupViewDeckButton;
        this.enableMaxHealthIndicator = enableMaxHealthIndicator;
        this.enableSelectedIndicator = enableSelectedIndicator;
        this.trimToJustMainBody = trimToJustMainBody;
        this.makeSpriteFillSpace = makeSpriteFillSpace;
        this.disableNametags = disableNametags;
        this.smallNametag = smallNametag;
        this.aspectRatio = aspectRatio;
        this.screenWidthPercent = screenWidthPercent;
        this.preventDefaultDeckViewButton = preventDefaultDeckViewButton;
        this.purelyVisual = purelyVisual;
    }
}