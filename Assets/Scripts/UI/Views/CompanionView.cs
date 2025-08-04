using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionView : IUIEventReceiver
{
    public static CompanionViewContext COMBAT_CONTEXT = 
        new CompanionViewContext(true, false, true, true, false, false, false, false, 1.25f, 0.2f);
    
    public static CompanionViewContext SHOP_CONTEXT = 
        new CompanionViewContext(false, true, false, false, true, false, false, true, 0.8f, 0.15f);
    
    public static CompanionViewContext UNIT_MNGMT_CONTEXT = 
        new CompanionViewContext(false, false, false, false, true, true, true, false, 1.1f, 0.15f);

    public static CompanionViewContext CARD_SELECTION_CONTEXT = 
        new CompanionViewContext(false, false, true, true, false, false, false, false, 1.25f, 0.2f);
    
    public static CompanionViewContext COMPENDIUM_CONTEXT = 
        new CompanionViewContext(false, false, false, true, false, false, false, false, 1.25f, 0.2f);

    public VisualElement container;
    public VisualElementFocusable focusable;

    private IUIEntity entity;
    private IEntityViewDelegate viewDelegate;
    private VisualTreeAsset template;
    private int index;
    private CompanionViewContext context;
    private CombatInstance combatInstance;

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

    public CompanionView(
            IUIEntity entity,
            VisualTreeAsset template,
            int index,
            CompanionViewContext context,
            IEntityViewDelegate viewDelegate) {
        this.entity = entity;
        this.viewDelegate = viewDelegate;
        this.template = template;
        this.index = index;
        this.context = context;

        this.combatInstance = entity.GetCombatInstance();
        
        SetupCompanionView();

        if (this.combatInstance) {
            combatInstance.onDamageHandler += DamageScaleBump;
            combatInstance.onDeathHandler +=  OnDeathHandler;
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
        Companion companion = null;
        if (this.entity is Companion comp) {
            companion = comp;
        } else if (this.entity is CompanionInstance instance) {
            companion = instance.companion;
        }

        switch (companion.companionType.level) {
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
        Companion companion = null;
        if (this.entity is Companion comp) {
            companion = comp;
        } else if (this.entity is CompanionInstance instance) {
            companion = instance.companion;
        }

        switch (companion.companionType.rarity) {
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
        this.solidBackground.style.backgroundImage = new StyleBackground(this.entity.GetBackgroundImage());
    }

    private void SetupBlockAndHealth() {
        if (this.combatInstance == null) {
            this.healthLabel.text = this.entity.GetCurrentHealth().ToString();
            this.maxHealthLabel.text = this.entity.GetCombatStats().getMaxHealth().ToString();
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

    private void SetupStatusIndicators() {
        this.statusContainer.Clear();

        if (this.combatInstance == null) return;

        foreach (KeyValuePair<StatusEffectType, int> kvp in combatInstance.GetDisplayedStatusEffects()) {
            // Block is displayed on the companion frame now :)
            if (kvp.Key == StatusEffectType.Defended) continue;
            this.statusContainer.Add(CreateStatusIndicator(viewDelegate.GetStatusEffectSprite(kvp.Key), kvp.Value.ToString()));
        }

        List<DisplayedCacheValue> cacheValues = combatInstance.GetDisplayedCacheValues();
        foreach (DisplayedCacheValue cacheValue in cacheValues) {
            this.statusContainer.Add(CreateStatusIndicator(cacheValue.sprite, cacheValue.value.ToString()));
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
        this.container.RegisterOnSelected(() => ContainerPointerClick(null));
        this.container.RegisterCallback<PointerEnterEvent>(ContainerPointerEnter);
        this.container.RegisterCallback<PointerLeaveEvent>(ContainerPointerLeave);

        this.focusable = this.container.AsFocusable();
        this.focusable.additionalFocusAction += () => ContainerPointerEnter(null);
        this.focusable.additionalUnfocusAction += () => ContainerPointerLeave(null);

        this.focusable.SetInputAction(GFGInputAction.VIEW_DECK, () => DrawPileButtonOnClick(null));
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
            Targetable targetable = this.entity.GetTargetable();
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
            Targetable targetable = this.entity.GetTargetable();
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
            Targetable targetable = this.entity.GetTargetable();
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
        viewDelegate.InstantiateCardView(new List<Card>(), "");
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

    private void DrawPileButtonOnClick(ClickEvent evt) {
        if (evt != null) evt.StopPropagation();
        Debug.Log("Draw button clicked");
        DeckInstance deckInstance = this.entity.GetDeckInstance();
        if (deckInstance == null) {
            Debug.LogError("Entity " + this.entity.GetName() + " does not have a deck instance, which is crazy, because it's clearly a companion");
            return;
        }
        viewDelegate.InstantiateCardView(deckInstance.GetShuffledDrawPile(), deckInstance.combatInstance.name + " draw pile");
    }

    private void DiscardPileButtonOnClick(ClickEvent evt) {
        if (evt != null) evt.StopPropagation();
        Debug.Log("Discard button clicked");
        DeckInstance deckInstance = this.entity.GetDeckInstance();
        if (deckInstance == null) {
            Debug.LogError("Entity " + this.entity.GetName() + " does not have a deck instance, which is crazy, because it's clearly a companion");
            return;
        }
        viewDelegate.InstantiateCardView(deckInstance.GetShuffledDiscardPile(), deckInstance.combatInstance.name + " discard pile");
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
        if (evt != null) elementsKeepingHiddenContainerVisible.Add(evt.currentTarget as VisualElement);
        // this.containerThatHoverIndicatorShows.style.visibility = Visibility.Visible;
        this.containerThatHoverIndicatorShows.style.display = DisplayStyle.Flex;
    }

    public void HoverDetectorPointerLeave(PointerLeaveEvent evt) {
        // This null check exists because ShopItemView will call this with a null event
        // if a shop item is hovered with non mouse controls
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
        Sprite sprite = null;
        if (entity is Companion companion) {
            sprite = companion.companionType.sprite;
        } else if (entity is CompanionInstance companionInstance) {
            sprite = companionInstance.companion.companionType.sprite;
        }
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
        this.primaryNameLabel.text = entity.GetName();
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
            ve.pickingMode = PickingMode.Position;
        }
    }

    private void DamageScaleBump(int scale) {
        if (scale == 0) return; // this could mean the damage didn't go through the block or that the companion died while taking damage

        float duration = 0.125f;  // Total duration for the scale animation
        float minScale = .8f; // (float)Math.Min(.75, .9 - scale / 500);  // scale bump increases in intensity if entity takes more damage (haven't extensively tested this)

        Vector2 originalElementScale = new Vector2(
            this.container.style.scale.value.value.x,
            this.container.style.scale.value.value.y
        );
        
        LeanTween.value(1f, minScale, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1) // inverse tween is called when this tween completes. On complete below is called after both tweens complete
            .setOnUpdate((float currentScale) => {
                this.container.style.scale = new StyleScale(new Scale(originalElementScale * currentScale));
            })
            .setOnComplete(() =>
            {
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
            float screenWidthPercent) {
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
    }
}