using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionView : IUIEventReceiver
{
    // wdith / height
    private static float CONTAINER_ASPECT_RATIO_FULL = 1.25f;
    private static float CONTAINER_ASPECT_RATIO_NARROW = 0.8f;
    private static float CONTAINER_ASPECT_RATIO_COMP_MGMT = 1.1f;
    private static float SCREEN_WIDTH_PERCENT_COMBAT = 0.20f;
    private static float SCREEN_WIDTH_PERCENT_SHOP = 0.15f;

    public VisualElement container;
    public VisualElementFocusable focusable;

    private IUIEntity entity;
    private IEntityViewDelegate viewDelegate;
    private VisualTreeAsset template;
    private int index;
    private CompanionViewType viewType;
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
    private VisualElement selectedIndicator;

    private List<VisualElement> pickingModePositionList = new List<VisualElement>();

    private List<VisualElement> elementsKeepingHiddenContainerVisible = new List<VisualElement>();
    private bool isDead = false;
    private VisualElement containerThatHoverIndicatorShows;

    public CompanionView(
            IUIEntity entity,
            VisualTreeAsset template,
            int index,
            CompanionViewType viewType,
            IEntityViewDelegate viewDelegate) {
        this.entity = entity;
        this.viewDelegate = viewDelegate;
        this.template = template;
        this.index = index;
        this.viewType = viewType;

        this.combatInstance = entity.GetCombatInstance();

        bool setupDrawDiscardButtons = false;
        bool setupViewDeckButton = false;
        switch (this.viewType) {
            case CompanionViewType.COMBAT:
                setupDrawDiscardButtons = true;
            break;

            case CompanionViewType.SHOP:
                setupViewDeckButton = true;
            break;

            case CompanionViewType.COMPANION_MANAGEMENT:
            case CompanionViewType.INFO_VIEW:
            break;
        }
        
        SetupCompanionView(setupDrawDiscardButtons, setupViewDeckButton);

        if (this.combatInstance) {
            combatInstance.onDamageHandler += DamageScaleBump;
            combatInstance.onDeathHandler +=  OnDeathHandler;
            combatInstance.SetVisualElement(this.container);
        }
    }

    private void SetupCompanionView(bool setupDrawDiscardButtons, bool setupViewDeckButton)
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
        this.lowerHoverDetector = companionRoot.Q<VisualElement>("companion-view-lower-hover-detector");
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
        SetupBackground();
        SetupCompanionSprite();
        SetupName();
        SetupBlockAndHealth();
        SetupStatusIndicators();

        if (setupDrawDiscardButtons || setupViewDeckButton) SetupHoverDetector();
        if (setupDrawDiscardButtons) SetupDrawDiscardContainer();
        if (setupViewDeckButton) SetupViewDeckContainer();


        UpdateWidthAndHeight(container);

    }

    private void SetupBackground() {
        this.solidBackground.style.backgroundImage = new StyleBackground(this.entity.GetBackgroundImage());
    }

    private void SetupBlockAndHealth() {
        if (this.combatInstance == null) {
            this.healthLabel.text = this.entity.GetCurrentHealth().ToString();
            this.blockLabel.style.visibility = Visibility.Hidden;
            return;
        }

        this.healthLabel.text = this.combatInstance.combatStats.currentHealth.ToString();
        this.blockLabel.text = this.combatInstance.GetStatus(StatusEffectType.Defended).ToString();
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
        this.focusable.additionalFocusAction += () => ContainerPointerEnter(this.container.CreateFakePointerEnterEvent());
        this.focusable.additionalUnfocusAction += () => ContainerPointerLeave(this.container.CreateFakePointerLeaveEvent());

        this.focusable.SetInputAction(GFGInputAction.VIEW_DECK, () => DrawPileButtonOnClick(null));
        this.focusable.SetInputAction(GFGInputAction.VIEW_DISCARD, () => DiscardPileButtonOnClick(null));

        if (this.viewType == CompanionViewType.SHOP || this.viewType == CompanionViewType.COMPANION_MANAGEMENT) {
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
        if (this.viewType == CompanionViewType.COMBAT) this.selectedIndicator.style.visibility = Visibility.Visible;

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
        viewDelegate.GetMonoBehaviour().StartCoroutine(HideContainerAtEndOfFrame());
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
        this.containerThatHoverIndicatorShows.style.visibility = Visibility.Visible;
    }

    public void HoverDetectorPointerLeave(PointerLeaveEvent evt) {
        // This null check exists because ShopItemView will call this with a null event
        // if a shop item is hovered with non mouse controls
        if (evt != null) elementsKeepingHiddenContainerVisible.Remove(evt.currentTarget as VisualElement);
        viewDelegate.GetMonoBehaviour().StartCoroutine(HideContainerAtEndOfFrame());
    }

    private IEnumerator HideContainerAtEndOfFrame() {
        yield return new WaitForEndOfFrame();
        if (elementsKeepingHiddenContainerVisible.Count == 0) {
            this.containerThatHoverIndicatorShows.style.visibility = Visibility.Hidden;
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
        if (this.viewType == CompanionViewType.COMPANION_MANAGEMENT) {
            this.imageElement.AddToClassList("companion-view-companion-image-fill-space");
        }
    }

    private void SetupName() {
        if (this.viewType == CompanionViewType.COMPANION_MANAGEMENT) {
            this.primaryNameLabel.style.display = DisplayStyle.None;
            this.secondaryNameLabel.style.display = DisplayStyle.None;
            return;
        } else if (this.viewType == CompanionViewType.SHOP) {
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
        float aspectRatio;
        float screenWidthPercent;
        switch (this.viewType) {
            case CompanionViewType.COMBAT:
            case CompanionViewType.INFO_VIEW:
                aspectRatio = CONTAINER_ASPECT_RATIO_FULL;
                screenWidthPercent = SCREEN_WIDTH_PERCENT_COMBAT;
            break;

            case CompanionViewType.SHOP:
                aspectRatio = CONTAINER_ASPECT_RATIO_NARROW;
                screenWidthPercent = SCREEN_WIDTH_PERCENT_SHOP;
            break;

            case CompanionViewType.COMPANION_MANAGEMENT:
            default:
                aspectRatio = CONTAINER_ASPECT_RATIO_COMP_MGMT;
                screenWidthPercent = SCREEN_WIDTH_PERCENT_SHOP;
            break;
        }

        int width = (int)(Screen.width * screenWidthPercent * scale);
        int height = (int)(width / aspectRatio);

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * screenWidthPercent * scale);
        height = (int)(width / aspectRatio);
        #endif

        return new Tuple<int, int>(width, height);
    }

    public void SetPickingModes(bool enable) {
        foreach (VisualElement ve in pickingModePositionList) {
            UIDocumentUtils.SetAllPickingMode(ve, enable ? PickingMode.Position : PickingMode.Ignore);
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
}

public enum CompanionViewType {
    COMBAT,
    SHOP,
    COMPANION_MANAGEMENT,
    INFO_VIEW
}