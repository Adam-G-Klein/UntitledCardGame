using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionView : IUIEventReceiver
{
    private static float CONTAINER_ASPECT_RATIO = 1.25f;
    private static float SCREEN_WIDTH_PERCENT = 0.20f;

    public VisualElement container;
    public VisualElementFocusable focusable;

    private IUIEntity entity;
    private IEntityViewDelegate viewDelegate;
    private VisualTreeAsset template;
    private int index;
    private CombatInstance combatInstance;

    private VisualElement parentContainer;
    private VisualElement statusContainer;
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
            bool setupDrawDiscardButtons,
            bool setupViewDeckButton,
            IEntityViewDelegate viewDelegate) {
        this.entity = entity;
        this.viewDelegate = viewDelegate;
        this.template = template;
        this.index = index;

        this.combatInstance = entity.GetCombatInstance();
        
        SetupCompanionView(setupDrawDiscardButtons, setupViewDeckButton);

        if (this.combatInstance) {
            combatInstance.onDamageHandler += DamageScaleBump;
            combatInstance.onDeathHandler +=  OnDeathHandler;
            combatInstance.SetVisualElement(this.container);
        }
    }

    private void SetupCompanionView(bool setupDrawDiscardButtons, bool setupViewDeckButton) {
        VisualElement companionRoot = this.template.CloneTree();

        this.parentContainer = companionRoot.Q<VisualElement>("companion-view-parent-container");
        this.statusContainer = companionRoot.Q<VisualElement>("companion-view-status-container");
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

        SetupCompanionSprite();
        SetupName();
        SetupBlockAndHealth();
        // SetupStatusIndicators();

        if (setupDrawDiscardButtons || setupViewDeckButton) SetupHoverDetector();
        if (setupDrawDiscardButtons) SetupDrawDiscardContainer();
        if (setupViewDeckButton) SetupViewDeckContainer();


        UpdateWidthAndHeight(container);

    }

    private void SetupBlockAndHealth() {
        if (this.combatInstance == null) return;

        this.healthLabel.text = this.combatInstance.combatStats.currentHealth.ToString();
        this.blockLabel.text = this.combatInstance.GetStatus(StatusEffectType.Defended).ToString();
    }

    private void SetupMainContainer() {
        this.container.RegisterOnSelected(() => ContainerPointerClick(null));
        this.container.RegisterCallback<PointerEnterEvent>(ContainerPointerEnter);
        this.container.RegisterCallback<PointerLeaveEvent>(ContainerPointerLeave);

        this.focusable = this.container.AsFocusable();
        this.focusable.additionalFocusAction += () => ContainerPointerEnter(this.container.CreateFakePointerEnterEvent());
        this.focusable.additionalUnfocusAction += () => ContainerPointerLeave(this.container.CreateFakePointerLeaveEvent());

        this.focusable.SetInputAction(GFGInputAction.VIEW_DECK, () => DrawButtonOnClick(null));
        this.focusable.SetInputAction(GFGInputAction.VIEW_DISCARD, () => DiscardButtonOnClick(null));
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

        this.selectedIndicator.style.visibility = Visibility.Visible;

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

    private void ContainerPointerLeave(PointerLeaveEvent evt) {
        if (isDead) return;

        this.selectedIndicator.style.visibility = Visibility.Hidden;

        try {
            Targetable targetable = this.entity.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerLeaveUI(evt);
        } catch (Exception e) {
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

        this.viewDeckButton.RegisterCallback<ClickEvent>(DrawButtonOnClick);

        this.containerThatHoverIndicatorShows = this.viewDeckContainer;
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

        this.drawPileButton.RegisterCallback<ClickEvent>(DrawButtonOnClick);
        this.discardPileButton.RegisterCallback<ClickEvent>(DiscardButtonOnClick);

        this.containerThatHoverIndicatorShows = this.drawDiscardContainer;
    }

    private void DrawButtonOnClick(ClickEvent evt) {
        if (evt != null) evt.StopPropagation();
        Debug.Log("Draw button clicked");
        DeckInstance deckInstance = this.entity.GetDeckInstance();
        if (deckInstance == null) {
            Debug.LogError("Entity " + this.entity.GetName() + " does not have a deck instance, which is crazy, because it's clearly a companion");
            return;
        }
        viewDelegate.InstantiateCardView(deckInstance.GetShuffledDrawPile(), deckInstance.combatInstance.name + " draw pile");
    }

    private void DiscardButtonOnClick(ClickEvent evt) {
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

    private void HoverDetectorPointerEnter(PointerEnterEvent evt) {
        if (this.isDead) return;
        elementsKeepingHiddenContainerVisible.Add(evt.currentTarget as VisualElement);
        this.containerThatHoverIndicatorShows.style.visibility = Visibility.Visible;
    }

    private void HoverDetectorPointerLeave(PointerLeaveEvent evt) {
        elementsKeepingHiddenContainerVisible.Remove(evt.currentTarget as VisualElement);
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
    }

    private void SetupName() {
        this.primaryNameLabel.text = entity.GetName();
        this.secondaryNameLabel.text = ""; // TODO: Do this lmao
    }

    public void UpdateWidthAndHeight(VisualElement root) {
        Tuple<int, int> entityWidthHeight = GetWidthAndHeight();
        root.style.width = entityWidthHeight.Item1;
        root.style.height = entityWidthHeight.Item2;
    }

    private Tuple<int, int> GetWidthAndHeight() {
        int width = (int)(Screen.width * SCREEN_WIDTH_PERCENT);
        int height = (int)(width / CONTAINER_ASPECT_RATIO);

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * SCREEN_WIDTH_PERCENT);
        height = (int)(width / CONTAINER_ASPECT_RATIO);
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
    }

    private IEnumerator OnDeathHandler(CombatInstance killer) {
        FocusManager.Instance.UnregisterFocusableTarget(this.focusable);
        isDead = true;
        yield return null;
    }
}
