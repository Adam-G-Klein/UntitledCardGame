using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class EntityView : IUIEventReceiver {
    public VisualElement entityContainer;
    public VisualElementFocusable elementFocusable;
    public IEntityViewDelegate viewDelegate;

    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";

    private float SCREEN_WIDTH_PERCENT = 0.11f;
    private float RATIO = 1.4f;
    private float ENEMY_RATIO = 1.65f;

    private IUIEntity uiEntity;
    private int index;
    private bool isEnemy;

    private List<VisualElement> pickingModePositionList = new List<VisualElement>();
    private List<VisualElement> elementsKeepingDrawDiscardVisible = new List<VisualElement>();

    private VisualElement pillar;
    private VisualElement drawDiscardContainer;
    private VisualElement healthAndBlockTab = null;
    private VisualElement statusEffectsTab = null;
    private VisualElement descriptionContainer = null;
    private CombatInstance combatInstance = null;
    private Vector3 originalScale;
    private Vector2 originalElementScale;
    private int ENTITY_NAME_MAX_CHARS = 6;
    private int ENTITY_NAME_FONT_SIZE = 20;

    public GameObject tweenTarget = new GameObject("ScaleBumpTarget");

    public EntityView(IUIEntity entity, int index, bool isEnemy, IEntityViewDelegate viewDelegate = null, bool isCompanionManagementView = false) {
        this.uiEntity = entity;
        this.index = index;
        this.isEnemy = isEnemy;
        this.viewDelegate = viewDelegate;
        entityContainer = setupEntity(entity, index, isEnemy, isCompanionManagementView);
        combatInstance = entity.GetCombatInstance();
        if (combatInstance) {
            combatInstance.onDamageHandler += DamageScaleBump;
        }
    }

    public void SetupEntityImage(Sprite sprite) {
        VisualElement portraitContainer = entityContainer.Q(className: "entity-portrait");
        portraitContainer.style.backgroundImage = new StyleBackground(sprite);
    }

    public void HideDescription() {
        entityContainer.Q("description-container").RemoveFromHierarchy();
        VisualElement detailsContainer = entityContainer.Q("details-container");
        detailsContainer.AddToClassList("pillar-details-no-description");
        detailsContainer.RemoveFromClassList("pillar-details");
        VisualElement titleContainer = entityContainer.Q("title-container");
        titleContainer.RemoveFromClassList("pillar-name");
        titleContainer.AddToClassList("pillar-name-no-description");
    }

    public void AddDrawDiscardOnHover() {
        if (isEnemy) return;

        // it's important to add the hover detector *before* the drawer,
        // or it will pick away the click events from the buttons
        VisualElement hoverDetector = new VisualElement();
        hoverDetector.AddToClassList("pillar-hover-box");
        pickingModePositionList.Add(hoverDetector);
        pillar.Add(hoverDetector);
        hoverDetector.RegisterCallback<PointerEnterEvent>(HoverDetectorOnPointerEnter);
        hoverDetector.RegisterCallback<PointerLeaveEvent>(HoverDetectorOnPointerLeave);

        drawDiscardContainer = setupCardDrawer(uiEntity);
        drawDiscardContainer.RegisterCallback<PointerEnterEvent>(DrawDiscardContainerOnPointerEnter);
        drawDiscardContainer.RegisterCallback<PointerLeaveEvent>(DrawDiscardContainerOnPointerLeave);
        pillar.Add(drawDiscardContainer);
    }

    public void UpdateWidthAndHeight(bool isEnemy = false) {
        Tuple<int, int> entityWidthHeight = GetWidthAndHeight(isEnemy);
        pillar.style.width = entityWidthHeight.Item1;
        pillar.style.height = entityWidthHeight.Item2;
    }

    public void UpdateView() {
        setupHealthAndBlockTabs(uiEntity);
        setupStatusEffectsTabs(uiEntity);
        if (uiEntity.GetEnemyInstance()) {
            setupEnemyIntentV2(uiEntity.GetEnemyInstance());
        };
    }

    private VisualElement setupEntity(IUIEntity entity, int index, bool isEnemy, bool isCompanionManagementView) {
        var pillar = new VisualElement();
        this.pillar = pillar;
        pillar.focusable = true;
        pillar.AddToClassList("entity-pillar");
        pillar.AddToClassList("entity-pillar-sizing");
        pillar.AddToClassList("focusable");
        elementFocusable = pillar.AsFocusable();
        elementFocusable.SetInputAction(GFGInputAction.VIEW_DECK, () => DrawButtonOnClick(null));
        elementFocusable.SetInputAction(GFGInputAction.VIEW_DISCARD, () => DiscardButtonOnClick(null));

        // pillar.RegisterCallback<ClickEvent>(EntityOnPointerClick);
        pillar.RegisterOnSelected(() => EntityOnPointerClick(null));
        pillar.RegisterCallback<PointerEnterEvent>(EntityOnPointerEnter);
        pillar.RegisterCallback<PointerLeaveEvent>(EntityOnPointerLeave);
        elementFocusable.additionalFocusAction += () => EntityOnPointerEnter(pillar.CreateFakePointerEnterEvent());
        elementFocusable.additionalUnfocusAction += () => EntityOnPointerLeave(pillar.CreateFakePointerLeaveEvent());
        pickingModePositionList.Add(pillar);

        var pillarContainer = new VisualElement();
        pillarContainer.AddToClassList("entity-pillar-container");

        VisualElement backgroundColor = new();
        backgroundColor.AddToClassList("entity-background-color");
        backgroundColor.style.backgroundImage = new StyleBackground(entity.GetBackgroundImage());
        pillarContainer.Add(backgroundColor);

        VisualElement portraitContainer = new();
        portraitContainer.AddToClassList("entity-portrait-container");

        VisualElement portrait = new();
        portrait.AddToClassList("entity-portrait");

        Sprite sprite;
        if (entity is Companion companion) {
            sprite = isCompanionManagementView ? companion.companionType.fullSprite : companion.companionType.sprite;
        } else if (entity is CompanionInstance companionInstance) {
            sprite = isCompanionManagementView ? companionInstance.companion.companionType.fullSprite : companionInstance.companion.companionType.sprite;
        } else {
            sprite = null;
        }
        portrait.style.backgroundImage = new StyleBackground(sprite);

        var baseString = isEnemy ? UIDocumentGameObjectPlacer.ENEMY_UIDOC_ELEMENT_PREFIX : UIDocumentGameObjectPlacer.COMPANION_UIDOC_ELEMENT_PREFIX;
        string portraitContainerName = baseString + index.ToString();
        portraitContainer.name = portraitContainerName;

        portraitContainer.Add(portrait);
        pillarContainer.Add(portraitContainer);

        VisualElement frame = new();
        frame.AddToClassList("entity-frame");
        if (isCompanionManagementView) {
            frame.style.backgroundImage = new StyleBackground(((Companion)entity).GetCompanionManagementViewFrame());
        } else {
            frame.style.backgroundImage = new StyleBackground(entity.GetEntityFrame());
        }
        pillarContainer.Add(frame);

        var leftColumn = new VisualElement();
        leftColumn.AddToClassList("pillar-left-column");
        leftColumn.Add(setupHealthAndBlockTabs(entity));
        pillarContainer.Add(leftColumn);

        var rightColumn = new VisualElement();
        rightColumn.AddToClassList("pillar-right-column");
        rightColumn.Add(setupStatusEffectsTabs(entity));
        pillarContainer.Add(rightColumn);

        if (!isCompanionManagementView) {
            VisualElement nameContainer = new();
            nameContainer.AddToClassList(isEnemy ? "entity-enemy-name-container": "entity-name-container");

            Label entityName = new();
            entityName.AddToClassList("entity-name");
            entityName.text = entity.GetName();
            entityName.style.fontSize = getNameFontSize(entity.GetName());
            nameContainer.Add(entityName);
            pillarContainer.Add(nameContainer);
        }

        if (isEnemy && entity.GetEnemyInstance()) {
            VisualElement descriptionContainer = setupEnemyIntentV2(entity.GetEnemyInstance());
            pillarContainer.Add(descriptionContainer);
        }

        pillar.Add(pillarContainer);
        UpdateWidthAndHeight(isEnemy);


        return pillar;
    }

    private VisualElement setupEnemyIntentV2(EnemyInstance enemyInstance) {
        if (descriptionContainer == null) {
            descriptionContainer = new VisualElement();
            descriptionContainer.name = "description-container";
            descriptionContainer.AddToClassList("enemy-intent-container");
        } else {
            descriptionContainer.Clear();
        }

        Label enemyIntentText = new();
        if(enemyInstance.currentIntent != null) {
            if (enemyInstance.currentIntent.GetDisplayValue() != 0) {
                String descriptionText = enemyInstance.currentIntent.GetDisplayValue().ToString();
                enemyIntentText.AddToClassList("enemy-intent-text");
                enemyIntentText.text = descriptionText;
                descriptionContainer.Add(enemyIntentText);
            }

            VisualElement intentImage = new();
            intentImage.AddToClassList("enemy-intent-icon");
            intentImage.style.backgroundImage = new StyleBackground(viewDelegate.GetEnemyIntentImage(enemyInstance.currentIntent.intentType));
            descriptionContainer.Add(intentImage);
        }

        return descriptionContainer;
    }

    private VisualElement setupHealthAndBlockTabs(IUIEntity entityInstance) {
        if (healthAndBlockTab == null) {
            healthAndBlockTab = new VisualElement();
            healthAndBlockTab.AddToClassList("pillar-tab-container");
        } else {
            healthAndBlockTab.Clear();
        }
        CombatStats stats = entityInstance.GetCombatStats();

        // Do an animate
        var healthTab = new VisualElement();
        healthTab.AddToClassList("health-tab");
        var healthLabel = new Label();
        healthLabel.AddToClassList("health-tab-text");
        healthLabel.text = stats.getCurrentHealth().ToString();

        var healthLabel2 = new Label();
        healthLabel2.AddToClassList("health-tab-text");
        healthLabel2.text = "---";

        var healthLabel3 = new Label();
        healthLabel3.AddToClassList("health-tab-text");
        healthLabel3.text = stats.getMaxHealth().ToString();

        healthTab.Add(healthLabel);
        healthTab.Add(healthLabel2);
        healthTab.Add(healthLabel3);
        healthAndBlockTab.Add(healthTab);

        CombatInstance combatInstance = entityInstance.GetCombatInstance();

        if (combatInstance && combatInstance.GetStatus(StatusEffectType.Defended) > 0) {
            var blockContainer = new VisualElement();
            blockContainer.AddToClassList("block-tab");
            var blockLabel = new Label();
            blockLabel.AddToClassList("pillar-tab-text");
            blockLabel.text = combatInstance.GetStatus(StatusEffectType.Defended).ToString();
            blockContainer.Add(blockLabel);
            healthAndBlockTab.Add(blockContainer);
        }

        return healthAndBlockTab;
    }

    private VisualElement setupStatusEffectsTabs(IUIEntity entityInstance) {
        if (statusEffectsTab == null) {
            statusEffectsTab = new VisualElement();
            statusEffectsTab.AddToClassList("pillar-tab-container");
        } else {
            statusEffectsTab.Clear();
        }

        CombatInstance combatInstance = entityInstance.GetCombatInstance();
        if (!combatInstance) {
            return statusEffectsTab;
        }

        Debug.Log("EntityInstance " + entityInstance.GetName() + " status effects: " + combatInstance.GetDisplayedStatusEffects());
        foreach (KeyValuePair<StatusEffectType, int> kvp in combatInstance.GetDisplayedStatusEffects()) {
            var statusEffectTab = new VisualElement();
            statusEffectTab.AddToClassList("status-tab");
            Sprite sprite = viewDelegate.GetStatusEffectSprite(kvp.Key);
            if(sprite == null) {
                Debug.LogError("Can't display status, StatusEffectSO does not have an image for " + kvp.Key.ToString());
                continue;
            }
            statusEffectTab.style.backgroundImage = new StyleBackground(sprite.texture);
            var statusEffectText = new Label();
            statusEffectText.text = kvp.Value.ToString();
            statusEffectText.AddToClassList("status-tab-text");
            statusEffectTab.Add(statusEffectText);
            statusEffectsTab.Add(statusEffectTab);
        }

        List<DisplayedCacheValue> cacheValues = combatInstance.GetDisplayedCacheValues();
        Debug.Log("EntityInstance " + entityInstance.GetName() + " cached values: " + cacheValues);
        foreach (DisplayedCacheValue cacheValue in cacheValues) {
            var statusEffectTab = new VisualElement();
            statusEffectTab.AddToClassList("status-tab");
            Sprite sprite = cacheValue.sprite;
            if(sprite == null) {
                Debug.LogError("Can't display cache value does not have an image for key " + cacheValue.key);
                continue;
            }
            statusEffectTab.style.backgroundImage = new StyleBackground(sprite.texture);
            var statusEffectText = new Label();
            statusEffectText.text = cacheValue.value.ToString();
            statusEffectText.AddToClassList("status-tab-text");
            statusEffectTab.Add(statusEffectText);
            statusEffectsTab.Add(statusEffectTab);
        }

        return statusEffectsTab;
    }

    private VisualElement setupCardDrawer(IUIEntity entity) {
        VisualElement drawerContainer = new VisualElement();
        drawerContainer.AddToClassList("pillar-drawer-menu");
        pickingModePositionList.Add(drawerContainer);

        Button drawButton = new Button();
        drawButton.AddToClassList("drawer-button");
        drawButton.text = "Draw";


        Button discardButton = new Button();
        discardButton.AddToClassList("drawer-button");
        discardButton.text = "Discard";

        pickingModePositionList.Add(drawButton);
        pickingModePositionList.Add(discardButton);
        pickingModePositionList.Add(drawerContainer);

        drawButton.RegisterCallback<ClickEvent>(DrawButtonOnClick);
        discardButton.RegisterCallback<ClickEvent>(DiscardButtonOnClick);

        drawerContainer.Add(drawButton);
        drawerContainer.Add(discardButton);

        drawerContainer.style.visibility = Visibility.Hidden;

        return drawerContainer;
    }

    private void EntityOnPointerClick(ClickEvent evt) {
        try {
            Targetable targetable = uiEntity.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerClickUI(evt);
        } catch (Exception e) {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," + 
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }

    private void EntityOnPointerEnter(PointerEnterEvent evt) {
        try {
            Targetable targetable = uiEntity.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerEnterUI(evt);
        } catch (Exception e) {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," + 
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }

    private void EntityOnPointerLeave(PointerLeaveEvent evt) {
        try {
            if (uiEntity == null) return;
            Targetable targetable = uiEntity.GetTargetable();
            if (targetable == null) return;
            targetable.OnPointerLeaveUI(evt);
        } catch (Exception e) {
            Debug.Log("EntityView: Caught exception while trying to get entity targetable," + 
                " might be due to the entity GO being destroyed");
            Debug.LogException(e);
        }
    }

    private void DrawButtonOnClick(ClickEvent evt) {
        if (evt != null) evt.StopPropagation();
        Debug.Log("Draw button clicked");
        DeckInstance deckInstance = uiEntity.GetDeckInstance();
        if (deckInstance == null) {
            Debug.LogError("Entity " + uiEntity.GetName() + " does not have a deck instance, which is crazy, because it's clearly a companion");
            return;
        }
        viewDelegate.InstantiateCardView(deckInstance.GetShuffledDrawPile(), deckInstance.combatInstance.name + " draw pile");
}

    private void DiscardButtonOnClick(ClickEvent evt) {
        if (evt != null) evt.StopPropagation();
        Debug.Log("Discard button clicked");
        DeckInstance deckInstance = uiEntity.GetDeckInstance();
        if (deckInstance == null) {
            Debug.LogError("Entity " + uiEntity.GetName() + " does not have a deck instance, which is crazy, because it's clearly a companion");
            return;
        }
        viewDelegate.InstantiateCardView(deckInstance.GetShuffledDiscardPile(), deckInstance.combatInstance.name + " discard pile");
    }

    private void HoverDetectorOnPointerEnter(PointerEnterEvent evt) {
        elementsKeepingDrawDiscardVisible.Add(evt.currentTarget as VisualElement);
        drawDiscardContainer.style.visibility = Visibility.Visible;
    }

    private void HoverDetectorOnPointerLeave(PointerLeaveEvent evt) {
        elementsKeepingDrawDiscardVisible.Remove(evt.currentTarget as VisualElement);
        viewDelegate.GetMonoBehaviour().StartCoroutine(HideDrawDiscardAtEndOfFrame());
    }

    private void DrawDiscardContainerOnPointerEnter(PointerEnterEvent evt) {
        // If we're entering the element, it's already visible due to the hover detector
        elementsKeepingDrawDiscardVisible.Add(evt.currentTarget as VisualElement);
    }

    private void DrawDiscardContainerOnPointerLeave(PointerLeaveEvent evt) {
        elementsKeepingDrawDiscardVisible.Remove(evt.currentTarget as VisualElement);
        viewDelegate.GetMonoBehaviour().StartCoroutine(HideDrawDiscardAtEndOfFrame());
    }

    private IEnumerator HideDrawDiscardAtEndOfFrame() {
        yield return new WaitForEndOfFrame();
        if (elementsKeepingDrawDiscardVisible.Count == 0) {
            drawDiscardContainer.style.visibility = Visibility.Hidden;
        }
    }

    private Tuple<int, int> GetWidthAndHeight(bool isEnemy) {
        int width = (int)(Screen.width * SCREEN_WIDTH_PERCENT);
        int height = (int)(isEnemy ? (width * ENEMY_RATIO) : (width * RATIO));

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * SCREEN_WIDTH_PERCENT);
        height = (int)(isEnemy ? (width * ENEMY_RATIO) : (width * RATIO));
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

        if (LeanTween.isTweening(tweenTarget)) return;
 
        Transform combatInstanceTransform = combatInstance.GetComponent<Transform>();
        originalScale = combatInstanceTransform.localScale;
        originalElementScale = new Vector2(
            entityContainer.style.scale.value.value.x,
            entityContainer.style.scale.value.value.y
        );

        VisualElement companionContainer = entityContainer.Q<VisualElement>(className: "entity-portrait-container");

        float duration = 0.125f;  // Total duration for the scale animation
        float minScale = .8f; //(float)Math.Min(.75, .9 - scale / 500);  // scale bump increases in intensity if entity takes more damage (haven't extensively tested this)
        
        LeanTween.value(tweenTarget, 1f, minScale, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(1) // inverse tween is called when this tween completes. On complete below is called after both tweens complete
            .setOnUpdate((float currentScale) => {
                entityContainer.style.scale = new StyleScale(new Scale(originalElementScale * currentScale));
                Vector3 entityWorldPosition = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(companionContainer);

                if (combatInstanceTransform != null) {
                    combatInstanceTransform.localScale = new Vector3(
                        originalScale.x * currentScale,
                        originalScale.y * currentScale,
                        originalScale.z
                    );
                    combatInstanceTransform.position = entityWorldPosition;
                }
            })
            .setOnComplete(() => {
                entityContainer.style.scale = new StyleScale(new Scale(originalElementScale));
                Vector3 entityWorldPosition = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(companionContainer);
                if (combatInstanceTransform != null) {
                    combatInstanceTransform.localScale = originalScale;
                    combatInstanceTransform.position = entityWorldPosition;
                }
            });
    }

    private int getNameFontSize(string entityName) {
        return UIDocumentUtils.UpdateTextSize(entityName, ENTITY_NAME_MAX_CHARS, ENTITY_NAME_FONT_SIZE);
    }
}