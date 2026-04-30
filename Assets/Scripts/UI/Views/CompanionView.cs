using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionView : IUIEventReceiver
{
    public VisualElement container;
    public VisualElementFocusable focusable;

    private ICompanionViewDelegate viewDelegate;
    private VisualTreeAsset template;
    private int index;
    private CompanionViewContext context;
    private CompanionInstance companionInstance = null;
    public CompanionInstance GetCompanionInstance() { return companionInstance; }
    private Companion companion = null;
    private CombatInstance combatInstance = null;
    private DeckInstance deckInstance = null;

    private VisualElement statusArea;
    private VisualElement spriteElement;
    private VisualElement frame;
    private VisualElement frameBackground;
    private Label name;
    private VisualElement healthBarFill;
    private VisualElement healthBarFillClip;
    private VisualElement healthBarParent;
    private Label healthBarLabel;
    private VisualElement drawDiscardContainer;
    private VisualElement viewDeckContainer;
    private IconButton drawPileButton;
    private IconButton discardPileButton;
    private IconButton viewDeckButton;
    private VisualElement hoverDetector;
    private VisualElement selectedIndicator;
    private VisualElement rarityIndicator;
    private VisualElement pilesContainer;
    private VisualElement drawPileIcon;
    private VisualElement discardPileIcon;
    private VisualElement healthBarBorder;

    private List<VisualElement> pickingModePositionList = new List<VisualElement>();
    private List<VisualElement> elementsKeepingHiddenContainerVisible = new List<VisualElement>();
    private bool isDead = false;
    private bool isFullyDisabled = false;
    private VisualElement containerThatHoverIndicatorShows;
    private bool isTweening = false;
    private IVisualElementScheduledItem animationItem;
    private int currentFrame;
    private List<Sprite> animationFrames;

    private int lastHealthValue;
    private bool isHealthTweening = false;
    private float currentRotationDegrees = 0f;

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
            this.deckInstance = this.companionInstance.deckInstance;
        }

        SetupCompanionView();

        if (this.combatInstance)
        {
            combatInstance.onDamageHandler += DamageScaleBump;
            combatInstance.onDeathHandler += OnDeathHandler;
            combatInstance.SetVisualElement(this.spriteElement);
        }
    }

    private void SetupCompanionView()
    {
        VisualElement companionRoot = this.template.CloneTree();

        this.statusArea = companionRoot.Q<VisualElement>("companion-view-status-area");
        this.spriteElement = companionRoot.Q<VisualElement>("companion-view-sprite");
        this.frame = companionRoot.Q<VisualElement>("companion-view-frame");
        this.frameBackground = companionRoot.Q<VisualElement>("companion-view-frame-background");
        this.name = companionRoot.Q<Label>("companion-view-name-label");
        this.healthBarFill = companionRoot.Q<VisualElement>("companion-view-health-bar-fill");
        this.healthBarFillClip = companionRoot.Q<VisualElement>("companion-view-health-bar-clip");
        this.healthBarParent = companionRoot.Q<VisualElement>("companion-view-health-bar-parent");
        this.healthBarLabel = companionRoot.Q<Label>("companion-view-health-bar-label");
        this.hoverDetector = companionRoot.Q<VisualElement>("companion-view-lower-hover-detector");
        this.drawDiscardContainer = companionRoot.Q<VisualElement>("companion-view-draw-discard-container");
        this.viewDeckContainer = companionRoot.Q<VisualElement>("companion-view-view-deck-container");
        this.drawPileButton = companionRoot.Q<IconButton>("companion-view-draw-pile-button");
        this.discardPileButton = companionRoot.Q<IconButton>("companion-view-discard-pile-button");
        this.viewDeckButton = companionRoot.Q<IconButton>("companion-view-view-deck-button");
        this.selectedIndicator = companionRoot.Q<VisualElement>("companion-view-selected-indicator");
        this.rarityIndicator = companionRoot.Q<VisualElement>("companion-view-rarity-icon");
        this.healthBarBorder = companionRoot.Q<VisualElement>("companion-view-health-bar-outline");

        // Moving past the random VisualElement parent CloneTree() creates
        this.container = companionRoot.Children().First();
        this.container.name = container.name + this.index;
        this.pickingModePositionList.Add(container);
        SetupRarityIndicator();
        SetupMainContainer();
        SetupCompanionSprite();
        SetupName();
        SetupHealth();
        SetupStatusIndicators();
        SetupFrame();

        if (this.context.setupDrawDiscardButtons || this.context.setupViewDeckButton) SetupHoverDetector();
        if (this.context.setupDrawDiscardButtons) SetupDrawDiscardContainer();
        if (this.context.setupViewDeckButton) SetupViewDeckContainer();
        if (this.context.setupPileIcons) SetupDrawDiscardPileIcons();

        UpdateWidthAndHeight(container);
    }

    public void UpdateView() {
        UpdateHealth();
        SetupStatusIndicators();
    }

    private void SetupDrawDiscardPileIcons() {
        pilesContainer = container.Q<VisualElement>("companion-view-deck-piles-container");
        drawPileIcon = container.Q<VisualElement>("companion-view-draw-pile");
        discardPileIcon = container.Q<VisualElement>("companion-view-discard-pile");
        drawPileIcon.RegisterCallback<ClickEvent>(DrawPileButtonOnClick);
        discardPileIcon.RegisterCallback<ClickEvent>(DiscardPileButtonOnClick);
        if (deckInstance != null) {
            deckInstance.OnDrawDiscardPilesChanged += OnDecksChangedHandler;
            deckInstance.OnDrawPileShuffled += ShuffleDiscardIntoDrawVFX;
            deckInstance.InvokeDrawDiscardPilesChanged();
            pilesContainer.RegisterCallback<GeometryChangedEvent>(SetDrawDiscardPilePositions);
        }
        pilesContainer.style.display = DisplayStyle.Flex;
        pickingModePositionList.Add(drawPileIcon);
        pickingModePositionList.Add(discardPileIcon);
    }

    private void SetDrawDiscardPilePositions(GeometryChangedEvent evt) {

        Vector3 drawPos = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(drawPileIcon);
        Vector3 discardPos = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(discardPileIcon);
        deckInstance.SetDrawDiscardPositions(drawPos, discardPos);
        pilesContainer.UnregisterCallback<GeometryChangedEvent>(SetDrawDiscardPilePositions);
    }

    private void OnDecksChangedHandler(List<Card> draw, List<Card> discard) {
        SetPileIcon(drawPileIcon, draw.Count);
        SetPileIcon(discardPileIcon, discard.Count);
    }

    private void SetPilesNumbers(int draw, int discard) {
        SetPileIcon(drawPileIcon, draw);
        SetPileIcon(discardPileIcon, discard);
    }

    private void SetPileIcon(VisualElement pile, int count) {
        pile.RemoveFromClassList("pile-no-cards");
        pile.RemoveFromClassList("pile-one-card");
        pile.RemoveFromClassList("pile-two-cards");
        pile.RemoveFromClassList("pile-three-cards");
        pile.RemoveFromClassList("pile-four-plus-cards");

        pile.style.unityBackgroundImageTintColor = count == 0 ? Color.white : companion.companionType.pack.packColor;

        if (count == 0) {
            pile.AddToClassList("pile-no-cards");
        } else if (count == 1) {
            pile.AddToClassList("pile-one-card");
        } else if (count == 2) {
            pile.AddToClassList("pile-two-cards");
        } else if (count == 3) {
            pile.AddToClassList("pile-three-cards");
        } else {
            pile.AddToClassList("pile-four-plus-cards");
        }
    }

    private void SetupFrame() {
        Sprite frameSprite = null;
        switch (this.companion.companionType.level) {
            case CompanionLevel.LevelThree:
                frameSprite = companion.companionType.pack.levelThreeFrame;
                healthBarBorder.AddToClassList("companion-health-bar-outline-gold");
            break;

            case CompanionLevel.LevelTwo:
                frameSprite = companion.companionType.pack.levelTwoFrame;
                healthBarBorder.AddToClassList("companion-health-bar-outline-silver");
            break;

            case CompanionLevel.LevelOne:
            default:
                frameSprite = companion.companionType.pack.levelOneFrame;
                healthBarBorder.AddToClassList("companion-health-bar-outline-copper");
            break;
        }

        this.frame.style.backgroundImage = new StyleBackground(frameSprite);
        this.frameBackground.style.backgroundImage = new StyleBackground(companion.companionType.pack.frameBackground);
    }

    private void SetupRarityIndicator() {
        Sprite rarityIndicator = null;
        PackSO pack = companion.companionType.pack;
        switch(this.companion.companionType.rarity) {
            case CompanionRarity.COMMON:
                rarityIndicator = pack.commonIcon;
                break;
            case CompanionRarity.UNCOMMON:
                rarityIndicator = pack.uncommonIcon;
                break;
            case CompanionRarity.RARE:
                rarityIndicator = pack.rareIcon;
                break;
        }

        this.rarityIndicator.style.backgroundImage = new StyleBackground(rarityIndicator);
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

        this.viewDeckButton.style.fontSize = context.viewDrawDiscardDeckFontSize;
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

        this.drawPileButton.style.fontSize = context.viewDrawDiscardDeckFontSize;
        this.discardPileButton.style.fontSize = context.viewDrawDiscardDeckFontSize;
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
        this.pickingModePositionList.Add(this.hoverDetector);
        this.hoverDetector.RegisterCallback<PointerEnterEvent>(HoverDetectorPointerEnter);
        this.hoverDetector.RegisterCallback<PointerLeaveEvent>(HoverDetectorPointerLeave);


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

    private void SetupStatusIndicators()
    {
        this.statusArea.Clear();

        if (this.combatInstance == null) return;

        foreach (KeyValuePair<StatusEffectType, int> kvp in combatInstance.GetDisplayedStatusEffects())
        {
            this.statusArea.Add(CreateStatusIndicator(viewDelegate.GetStatusEffectSprite(kvp.Key), kvp.Value.ToString()));
        }

        List<DisplayedCacheValue> cacheValues = combatInstance.GetDisplayedCacheValues();
        foreach (DisplayedCacheValue cacheValue in cacheValues)
        {
            this.statusArea.Add(CreateStatusIndicator(cacheValue.sprite, cacheValue.value.ToString()));
        }

        // Create visuals for the powers that don't display cached values.
        List<(PowerSO, int)> activePowers = combatInstance.GetPowersWithStackCounts();
        foreach ((PowerSO, int) p in activePowers)
        {
            if (p.Item1.cacheConfiguration.key != "" && p.Item1.cacheConfiguration.display) continue;
            this.statusArea.Add(CreateStatusIndicator(p.Item1.displaySprite, ""));
        }
    }

    private VisualElement CreateStatusIndicator(Sprite icon, string textValue) {
        VisualElement statusIndicator = new VisualElement();
        statusIndicator.AddToClassList("entity-view-status-indicator");

        Label statusLabel = new Label();
        statusLabel.AddToClassList("entity-view-status-indicator-label");
        statusLabel.text = textValue;

        VisualElement statusIcon = new VisualElement();
        statusIcon.AddToClassList("entity-view-status-indicator-icon");
        statusIcon.style.backgroundImage = new StyleBackground(icon);

        statusIndicator.Add(statusLabel);
        statusIndicator.Add(statusIcon);

        return statusIndicator;
    }

    private void UpdateHealth() {
        if (isHealthTweening) return;

        int currentHealth;
        int maxHealth;
        if (this.companionInstance == null) {
            currentHealth = this.companion.GetCurrentHealth();
            maxHealth = this.companion.GetCombatStats().getMaxHealth();
        } else {
            currentHealth = this.combatInstance.combatStats.currentHealth;
            maxHealth = this.combatInstance.combatStats.maxHealth;
        }

        if (currentHealth == lastHealthValue) return;

        isHealthTweening = true;

        HealthBarUtils.UpdateHealth(lastHealthValue, currentHealth, maxHealth, healthBarFillClip, healthBarLabel, () => {
                isHealthTweening = false;
                lastHealthValue = currentHealth;
                // In case multiple instances of damage come through in close timing
                UpdateHealth();
            });
    }

    private void SetupHealth() {
        int currentHealth;
        int maxHealth;
        if (this.companionInstance == null) {
            currentHealth = this.companion.GetCurrentHealth();
            maxHealth = this.companion.GetCombatStats().getMaxHealth();
        } else {
            currentHealth = this.combatInstance.combatStats.currentHealth;
            maxHealth = this.combatInstance.combatStats.maxHealth;
        }
        HealthBarUtils.SetupHealth(currentHealth, maxHealth, healthBarFillClip, healthBarLabel);
        lastHealthValue = currentHealth;
        container.RegisterCallback<GeometryChangedEvent>(SetHealthBarFillSize);
    }

    private void SetHealthBarFillSize(GeometryChangedEvent evt) {
        healthBarFill.style.width = healthBarParent.resolvedStyle.width;
        healthBarFill.style.height = healthBarParent.resolvedStyle.height;
        container.UnregisterCallback<GeometryChangedEvent>(SetHealthBarFillSize);
    }

    private void SetupName() {
        this.name.text = this.companion.GetName();
        this.name.style.fontSize = context.nameFontSize;
    }

    private void SetupCompanionSprite() {
        if (this.companion.companionType.animationSprites == null || this.companion.companionType.animationSprites.Count == 0) {
            SetStaticSprite();
            return;
        }

        animationFrames = this.companion.companionType.animationSprites;
        currentFrame = 0;
        this.spriteElement.style.backgroundImage = new StyleBackground(animationFrames[0]);
        // this.spriteElement.style.rotate = new StyleRotate(new Rotate(new Angle(180, AngleUnit.Degree)));
        animationItem = this.spriteElement.schedule.Execute(() => {
            currentFrame = (currentFrame + 1) % animationFrames.Count;
            this.spriteElement.style.backgroundImage = new StyleBackground(animationFrames[currentFrame]);
        }).Every(250);
    }

    public void RotateSprite(float angle) {
        currentRotationDegrees += angle;
        currentRotationDegrees = currentRotationDegrees % 360;
        this.spriteElement.style.rotate = new StyleRotate(new Rotate(new Angle(currentRotationDegrees, AngleUnit.Degree)));
    }

    private void SetStaticSprite() {
        Sprite sprite = this.companion.companionType.fullSprite;
        if (sprite == null) {
            sprite = this.companion.getSprite();
        }
        this.spriteElement.style.backgroundImage = new StyleBackground(sprite);
    }

    private void SetupMainContainer() {
        this.container.RegisterOnSelected(() => ContainerPointerClick(null), false);
        this.container.RegisterCallback<PointerEnterEvent>(ContainerPointerEnter);
        this.container.RegisterCallback<PointerLeaveEvent>(ContainerPointerLeave);

        focusable = this.container.AsFocusable();
        focusable.additionalFocusAction += () => ContainerPointerEnter(null);
        focusable.additionalUnfocusAction += () => ContainerPointerLeave(null);

        focusable.SetInputAction(GFGInputAction.VIEW_DECK, () => {
            if (this.context.setupViewDeckButton) ViewDeckButtonOnClick(null);
            else if (this.context.setupDrawDiscardButtons) DrawPileButtonOnClick(null);
            else return;
        });
        focusable.SetInputAction(GFGInputAction.VIEW_DISCARD, () => DiscardPileButtonOnClick(null));
    }

    public void SetSelectionIndicatorVisibility(bool visible)
    {
        this.selectedIndicator.visible = visible;
    }

    private void ViewDeckButtonOnClick(ClickEvent evt) {
        if (evt != null) evt.StopPropagation();
        // Luke needs to fix this because it's bad but I'm in a rush
        viewDelegate.ViewDeck(DeckViewType.EntireDeck, companion);
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

    private void ContainerPointerEnter(PointerEnterEvent evt) {
        if (isDead) return;

        // Shop does it's own thing for hovering over companions
        if (this.context.enableSelectedIndicator)
            this.selectedIndicator.style.visibility = Visibility.Visible;

        try {
            if (this.companionInstance == null) return;
            //PlayerHand.Instance.HighlightRelevantCards(this.companionInstance.deckInstance);
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

        try
        {
            if (this.companionInstance == null) return;
            //PlayerHand.Instance.HighlightRelevantCards(null);
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

    private IEnumerator OnDeathHandler(CombatInstance killer) {
        FocusManager.Instance.UnregisterFocusableTarget(this.focusable);
        isDead = true;
        deckInstance.OnDrawDiscardPilesChanged -= OnDecksChangedHandler;
        pilesContainer.style.display = DisplayStyle.None;
        animationItem?.Pause();
        SetStaticSprite();
        yield return null;
    }

    public bool IsDead() {
        return isDead;
    }

    private void DamageScaleBump(int scale) {
        if (scale == 0 || this.isTweening) return; // this could mean the damage didn't go through the block or that the companion died while taking damage

        float duration = 0.125f;  // Total duration for the scale animation
        float minScale = .8f; // (float)Math.Min(.75, .9 - scale / 500);  // scale bump increases in intensity if entity takes more damage (haven't extensively tested this)

        Vector2 originalElementScale = new Vector2(
            this.spriteElement.style.scale.value.value.x,
            this.spriteElement.style.scale.value.value.y
        );

        LeanTween.value(1f, minScale, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1) // inverse tween is called when this tween completes. On complete below is called after both tweens complete
            .setOnUpdate((float currentScale) =>
            {
                this.spriteElement.style.scale = new StyleScale(new Scale(originalElementScale * currentScale));
            })
            .setOnStart(() =>
            {
                this.isTweening = true;
            })
            .setOnComplete(() =>
            {
                this.isTweening = false;
                this.spriteElement.style.scale = new StyleScale(new Scale(originalElementScale));
            });
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
        int height = width; // 1:1 aspect ratio

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * this.context.screenWidthPercent * scale);
        height = width; // 1:1 aspect ratio
        #endif

        return new Tuple<int, int>(width, height);
    }

    public void SetPickingModes(bool enable) {
        if (isFullyDisabled) return;
        foreach (VisualElement ve in pickingModePositionList) {
            ve.pickingMode = enable ? PickingMode.Position : PickingMode.Ignore;
        }
    }

    public void DisableInteractions() {
        UIDocumentUtils.SetAllPickingMode(this.container, PickingMode.Ignore);
        this.isFullyDisabled = true;
    }

    public void EnableInteractions() {
        this.isFullyDisabled = false;
        SetPickingModes(true);
    }

    public IEnumerator AbilityActivatedVFX() {
        VisualElement spriteCopy = new VisualElement();
        spriteCopy.style.backgroundImage = new StyleBackground(this.companion.companionType.fullSprite);
        // maybe using flex grow 1 instead would work here :thinking:
        spriteCopy.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Contain));
        spriteCopy.style.width = new Length(100, LengthUnit.Percent);
        spriteCopy.style.height = new Length(100, LengthUnit.Percent);
        spriteCopy.style.rotate = new StyleRotate(new Rotate(new Angle(currentRotationDegrees, AngleUnit.Degree)));
        yield return EntityAbilityInstance.GenericAbilityTriggeredVFX(this.spriteElement, spriteCopy);
    }

    public IEnumerator ShuffleDiscardIntoDrawVFX(int cardsShuffled) {
        Vector2 start = GetElementCenterInParentSpace(discardPileIcon, container);
        Vector2 end = GetElementCenterInParentSpace(drawPileIcon, container);
        int cardsInDiscard = cardsShuffled;
        int cardsInDraw = 0;
        List<VisualElement> tweens = new List<VisualElement>();
        for (int i = 0; i < cardsShuffled; i++) {
            VisualElement card = new VisualElement();
            card.AddToClassList("companion-view-deck-shuffled-card");
            card.style.unityBackgroundImageTintColor = companion.companionType.pack.packColor;
            card.style.width = discardPileIcon.resolvedStyle.width;
            card.style.height = discardPileIcon.resolvedStyle.height;
            container.Add(card);
            float arcHeight = UnityEngine.Random.Range(5, 35);
            cardsInDiscard -= 1;
            tweens.Add(card);
            LeanTween.value(0f, 1f, 0.2f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnUpdate((float t) => {
                    Vector2 pos = EvaluateArc(start, end, arcHeight, t);
                    SetElementCenter(card, pos);
                    float rot = Mathf.Lerp(-8f, 8f, t);
                    card.style.rotate = new StyleRotate(new Rotate(new Angle(rot)));
                })
                .setOnComplete(() => {
                    card.RemoveFromHierarchy();
                    cardsInDraw += 1;
                    tweens.Remove(card);
                });
            SetPilesNumbers(cardsInDraw, cardsInDiscard);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitUntil(() => tweens.Count == 0);
    }

    private static Vector2 EvaluateArc(Vector2 start, Vector2 end, float arcHeight, float t)
    {
        // Linear X/Y from start to end
        Vector2 pos = Vector2.Lerp(start, end, t);

        // Parabolic offset:
        // 0 at t=0 and t=1, max at t=0.5
        float arcOffset = 4f * arcHeight * t * (1f - t);

        // UI Toolkit Y grows downward, so subtract to go upward
        pos.y -= arcOffset;

        return pos;
    }

    private static void SetElementCenter(VisualElement element, Vector2 center)
    {
        float width = Mathf.Max(element.resolvedStyle.width, 1f);
        float height = Mathf.Max(element.resolvedStyle.height, 1f);

        element.style.left = center.x - width * 0.5f;
        element.style.top = center.y - height * 0.5f;
    }

    private static Vector2 GetElementCenterInParentSpace(VisualElement element, VisualElement targetParent)
    {
        Rect worldBound = element.worldBound;
        Vector2 worldCenter = worldBound.center;

        Vector2 localCenter = targetParent.WorldToLocal(worldCenter);
        return localCenter;
    }

    public void SetEverythingHidden() {
        container.style.visibility = Visibility.Hidden;
        pilesContainer.style.visibility = Visibility.Hidden;
    }

    public void SetOnlySpriteVisible() {
        container.style.visibility = Visibility.Hidden;
        pilesContainer.style.visibility = Visibility.Hidden;
        spriteElement.style.visibility = Visibility.Visible;
    }

    public void SetEverythingVisible() {
        container.style.visibility = Visibility.Visible;
        pilesContainer.style.visibility = Visibility.Visible;
    }

    public void FadeInFrame(float seconds) {
        spriteElement.style.opacity = 1f;
        LeanTween.value(0f, 1f, seconds)
            .setOnUpdate((float val) => {
                frame.style.opacity = val;
                SetEverythingVisible();
            });
    }

    public class CompanionViewContext {
        public bool setupDrawDiscardButtons;
        public bool setupViewDeckButton;
        public bool setupPileIcons;
        public bool enableSelectedIndicator;
        public int nameFontSize;
        public int viewDrawDiscardDeckFontSize;
        public float screenWidthPercent;

        public CompanionViewContext(
                bool setupDrawDiscardButtons,
                bool setupViewDeckButton,
                bool setupPileIcons,
                bool enableSelectedIndicator,
                int nameFontSize,
                int viewDrawDiscardDeckFontSize,
                float screenWidthPercent) {
            this.setupDrawDiscardButtons = setupDrawDiscardButtons;
            this.setupViewDeckButton = setupViewDeckButton;
            this.setupPileIcons = setupPileIcons;
            this.enableSelectedIndicator = enableSelectedIndicator;
            this.nameFontSize = nameFontSize;
            this.viewDrawDiscardDeckFontSize = viewDrawDiscardDeckFontSize;
            this.screenWidthPercent = screenWidthPercent;
        }
    }

    public static CompanionViewContext COMBAT_CONTEXT = new CompanionViewContext(
        false, false, true, true, 24, 20, 0.15f);
    public static CompanionViewContext VICTORY_CONTEXT = new CompanionViewContext(
        false, false, false, false, 24, 20, 0.15f);
    public static CompanionViewContext SHOP_CONTEXT = new CompanionViewContext(
        false, true, false, false, 18, 16, 0.15f * .75f);
    public static CompanionViewContext STARTING_TEAM_CONTEXT = new CompanionViewContext(
        false, true, false, false, 28, 20, 0.15f * .75f);
    public static CompanionViewContext COMPENDIUM_CONTEXT = new CompanionViewContext(
        false, false, false, false, 26, 20, 0.2f);
    public static CompanionViewContext CARD_SELECTION_CONTEXT = new CompanionViewContext(
        false, false, false, true, 26, 20, 0.2f);
    public static CompanionViewContext COMPANION_UPGRADE_CONTEXT = new CompanionViewContext(
        false, false, false, false, 21, 20, 0.15f);
}