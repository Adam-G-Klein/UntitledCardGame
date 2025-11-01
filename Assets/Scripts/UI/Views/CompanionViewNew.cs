using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionViewNew : IUIEventReceiver
{
    public static CompanionViewNewContext COMBAT_CONTEXT;
    public static CompanionViewNewContext SHOP_CONTEXT = new CompanionViewNewContext(
        false, true, false, 0.15f * .75f);

    public VisualElement container;
    public VisualElementFocusable focusable;

    private ICompanionViewDelegate viewDelegate;
    private VisualTreeAsset template;
    private int index;
    private CompanionViewNewContext context;
    private CompanionInstance companionInstance = null;
    private Companion companion = null;
    private CombatInstance combatInstance = null;

    private VisualElement statusArea;
    private VisualElement spriteElement;
    private VisualElement backgroundGradient;
    private VisualElement frame;
    private Label name;
    private VisualElement healthBarFill;
    private Label healthBarLabel;
    private VisualElement drawDiscardContainer;
    private VisualElement viewDeckContainer;
    private IconButton drawPileButton;
    private IconButton discardPileButton;
    private IconButton viewDeckButton;
    private VisualElement hoverDetector;
    private VisualElement selectedIndicator;

    private List<VisualElement> pickingModePositionList = new List<VisualElement>();
    private List<VisualElement> elementsKeepingHiddenContainerVisible = new List<VisualElement>();
    private bool isDead = false;
    private VisualElement containerThatHoverIndicatorShows;
    private bool isTweening = false;

    private static string HEALTH_LABEL_STRING = "{0}/{1}";

    public CompanionViewNew(
            Companion companion,
            VisualTreeAsset template,
            int index,
            CompanionViewNewContext context,
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

        if (this.combatInstance)
        {
            combatInstance.onDamageHandler += DamageScaleBump;
            combatInstance.onDeathHandler += OnDeathHandler;
            combatInstance.SetVisualElement(this.container);
        }
    }

    private void SetupCompanionView()
    {
        VisualElement companionRoot = this.template.CloneTree();

        this.statusArea = companionRoot.Q<VisualElement>("companion-view-status-area");
        this.spriteElement = companionRoot.Q<VisualElement>("companion-view-sprite");
        this.backgroundGradient = companionRoot.Q<VisualElement>("companion-view-background-gradient");
        this.frame = companionRoot.Q<VisualElement>("companion-view-frame");
        this.name = companionRoot.Q<Label>("companion-view-name-label");
        this.healthBarFill = companionRoot.Q<VisualElement>("companion-view-health-bar-fill");
        this.healthBarLabel = companionRoot.Q<Label>("companion-view-health-bar-label");
        this.hoverDetector = companionRoot.Q<VisualElement>("companion-view-lower-hover-detector");
        this.drawDiscardContainer = companionRoot.Q<VisualElement>("companion-view-draw-discard-container");
        this.viewDeckContainer = companionRoot.Q<VisualElement>("companion-view-view-deck-container");
        this.drawPileButton = companionRoot.Q<IconButton>("companion-view-draw-pile-button");
        this.discardPileButton = companionRoot.Q<IconButton>("companion-view-discard-pile-button");
        this.viewDeckButton = companionRoot.Q<IconButton>("companion-view-view-deck-button");
        this.selectedIndicator = companionRoot.Q<VisualElement>("companion-view-selected-indicator");

        // Moving past the random VisualElement parent CloneTree() creates
        this.container = companionRoot.Children().First();
        this.container.name = container.name + this.index;
        this.pickingModePositionList.Add(container);
        SetupMainContainer();
        SetupCompanionSprite();
        SetupName();
        SetupHealth();
        SetupStatusIndicators();
        SetupLevelIndicator();

        if (this.context.setupDrawDiscardButtons || this.context.setupViewDeckButton) SetupHoverDetector();
        if (this.context.setupDrawDiscardButtons) SetupDrawDiscardContainer();
        if (this.context.setupViewDeckButton) SetupViewDeckContainer();

        UpdateWidthAndHeight(container);
    }

    private void SetupLevelIndicator() {
        // TODO
        return;
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
            // Block is displayed on the companion frame now :)
            if (kvp.Key == StatusEffectType.Defended) continue;
            this.statusArea.Add(CreateStatusIndicator(viewDelegate.GetStatusEffectSprite(kvp.Key), kvp.Value.ToString()));
        }

        List<DisplayedCacheValue> cacheValues = combatInstance.GetDisplayedCacheValues();
        foreach (DisplayedCacheValue cacheValue in cacheValues)
        {
            this.statusArea.Add(CreateStatusIndicator(cacheValue.sprite, cacheValue.value.ToString()));
        }

        List<(PowerSO, int)> activePowers = combatInstance.GetPowersWithStackCounts();
        foreach ((PowerSO, int) p in activePowers)
        {
            string val = p.Item1.stackable ?  p.Item2.ToString() : "";
            this.statusArea.Add(CreateStatusIndicator(p.Item1.displaySprite, val));
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
        this.healthBarLabel.text = String.Format(HEALTH_LABEL_STRING, currentHealth, maxHealth);
        float healthPercent = (float) currentHealth / (float) maxHealth;
        this.healthBarFill.style.width = Length.Percent(healthPercent * 100);
    }

    private void SetupName() {
        this.name.text = this.companion.GetName();
    }

    private void SetupCompanionSprite() {
        Sprite sprite = this.companion.companionType.fullSprite;
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
        yield return null;
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

    public void SetPickingModes(bool inMenu)
    {
        throw new NotImplementedException();
    }

    public class CompanionViewNewContext {
        public bool setupDrawDiscardButtons;
        public bool setupViewDeckButton;
        public bool enableSelectedIndicator;
        public float screenWidthPercent;

        public CompanionViewNewContext(
                bool setupDrawDiscardButtons,
                bool setupViewDeckButton,
                bool enableSelectedIndicator,
                float screenWidthPercent) {
            this.setupDrawDiscardButtons = setupDrawDiscardButtons;
            this.setupViewDeckButton = setupViewDeckButton;
            this.enableSelectedIndicator = enableSelectedIndicator;
            this.screenWidthPercent = screenWidthPercent;
        }
    }
}