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
    public IEntityViewDelegate viewDelegate;

    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";

    private float SCREEN_WIDTH_PERCENT = 0.11f;
    private float RATIO = 1.4f;

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

    public EntityView(IUIEntity entity, int index, bool isEnemy, IEntityViewDelegate viewDelegate = null) {
        this.uiEntity = entity;
        this.index = index;
        this.isEnemy = isEnemy;
        this.viewDelegate = viewDelegate;
        entityContainer = setupEntity(entity, index, isEnemy);
        combatInstance = entity.GetCombatInstance();
        if (combatInstance) {
            combatInstance.onDamageHandler += DamageScaleBump;
        }
    }

    public void SetupEntityImage(Sprite sprite) {
        VisualElement portraitContainer = entityContainer.Q(className: "portrait-container");
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

    public void UpdateWidthAndHeight() {
        Tuple<int, int> entityWidthHeight = GetWidthAndHeight();
        pillar.style.width = entityWidthHeight.Item1;
        pillar.style.height = entityWidthHeight.Item2;
    }

    public void UpdateView() {
        setupHealthAndBlockTabs(uiEntity);
        setupStatusEffectsTabs(uiEntity);
        setupCardColumnDescription(uiEntity);
    }

    private VisualElement setupEntity(IUIEntity entity, int index, bool isEnemy) {
        var pillar = new VisualElement();
        this.pillar = pillar;
        pillar.AddToClassList("entity-pillar");
        pillar.AddToClassList("entity-pillar-sizing");
        pillar.RegisterCallback<ClickEvent>(EntityOnPointerClick);
        pillar.RegisterCallback<PointerEnterEvent>(EntityOnPointerEnter);
        pillar.RegisterCallback<PointerLeaveEvent>(EntityOnPointerLeave);
        pickingModePositionList.Add(pillar);


        var topTriangle = new VisualElement();
        topTriangle.AddToClassList("top-triangle");
        pillar.Add(topTriangle);

        var pillarContainer = new VisualElement();
        pillarContainer.AddToClassList("pillar-container");
        if (isEnemy) {
            pillarContainer.AddToClassList("enemy-pillar-container");
        }
        pillarContainer.Add(setupCardColumn(entity, index, isEnemy));

        var leftColumn = new VisualElement();
        leftColumn.AddToClassList("pillar-left-column");
        leftColumn.Add(setupHealthAndBlockTabs(entity));
        pillarContainer.Add(leftColumn);

        var rightColumn = new VisualElement();
        rightColumn.AddToClassList("pillar-right-column");
        rightColumn.Add(setupStatusEffectsTabs(entity));
        pillarContainer.Add(rightColumn);

        pillar.Add(pillarContainer);
        UpdateWidthAndHeight();
        
        return pillar;
    }

    private VisualElement setupCardColumn(IUIEntity entity, int index, bool isEnemy) {
        var column = new VisualElement();
        column.name = entity.GetName();
        column.AddToClassList("pillar-card-column");
        if (isEnemy) {
            column.AddToClassList("enemy-pillar-card-column");
        }

        var internalBorder = new VisualElement();
        internalBorder.AddToClassList("pillar-internal-border");
        column.Add(internalBorder);

        VisualElement detailsContainer = setupCardColumnPortraitAndTitle(column, entity, index, isEnemy);
        VisualElement descriptionContainer = setupCardColumnDescription(entity);
        
        detailsContainer.Add(descriptionContainer);

        column.Add(detailsContainer);

        return column;
    }

    // returns the details container, which holds everything below the portrait
    private VisualElement setupCardColumnPortraitAndTitle(VisualElement column, IUIEntity entity, int index, bool isEnemy) {
        var portraitContainerContainer = new VisualElement(); //The name makes sense I promise
        portraitContainerContainer.AddToClassList("portrait-container-container");

        var portraitContainer = new VisualElement();
        var baseString = isEnemy ? UIDocumentGameObjectPlacer.ENEMY_UIDOC_ELEMENT_PREFIX : UIDocumentGameObjectPlacer.COMPANION_UIDOC_ELEMENT_PREFIX;
        string portraitContainerName = baseString + index.ToString();
        portraitContainer.name = portraitContainerName;
        portraitContainer.AddToClassList("portrait-container");
        portraitContainerContainer.Add(portraitContainer);
        column.Add(portraitContainerContainer);
        column.AddToClassList(portraitContainer.name + STATUS_EFFECTS_CONTAINER_SUFFIX);

        var detailsContainer = new VisualElement();
        detailsContainer.name = "details-container";
        detailsContainer.AddToClassList("pillar-details");

        var titleContainer = new VisualElement();
        titleContainer.name = "title-container";
        titleContainer.AddToClassList("pillar-name");
        if (isEnemy) {
            titleContainer.AddToClassList("enemy-pillar-name");
        }
        var titleLabel = new Label();
        titleLabel.AddToClassList("pillar-name");
        if (isEnemy) {
            titleLabel.AddToClassList("enemy-pillar-name");
        }
        Debug.LogError("setting up entity");
        if (!isEnemy && (entity is Companion || entity is CompanionInstance)) {
            CompanionRarity rarity;
            if (entity is Companion) {
                rarity = ((Companion)entity).companionType.rarity;
            } else {
                rarity = ((CompanionInstance)entity).companion.companionType.rarity;
            }
            switch(rarity) {
                case CompanionRarity.COMMON:
                    titleLabel.AddToClassList("companion-rarity-bg-common");
                    break;
                case CompanionRarity.UNCOMMON:
                    titleLabel.AddToClassList("companion-rarity-bg-uncommon");
                    break;
                case CompanionRarity.RARE:
                    titleLabel.AddToClassList("companion-rarity-bg-rare");
                    break;
                }
        }
        titleContainer.Add(titleLabel);
        titleLabel.text = entity.GetName();
        detailsContainer.Add(titleContainer);
        return detailsContainer;
    }

    // returns the description container, which holds the enemy intent, the companion description, and the
    // deck drawers on hover for companions
    private VisualElement setupCardColumnDescription(IUIEntity entity) {
        if (descriptionContainer == null) {
            descriptionContainer = new VisualElement();
            descriptionContainer.name = "description-container";
            descriptionContainer.AddToClassList("pillar-text");
            pickingModePositionList.Add(descriptionContainer);
        } else {
            descriptionContainer.Clear();
        }

        var descLabel = new Label();

        EnemyInstance enemyInstance = entity.GetEnemyInstance();
        if (enemyInstance) {
            VisualElement innerContainer = new VisualElement();
            innerContainer.AddToClassList("enemy-intent-inner-container");
            setupEnemyIntent(descLabel, innerContainer, enemyInstance);
            innerContainer.Add(descLabel);
            descriptionContainer.Add(innerContainer);
        } else { // then we know it's a companion
            descLabel.AddToClassList("pillar-desc-label");
            descLabel.text = entity.GetDescription();
            descriptionContainer.Add(descLabel);
        }
        return descriptionContainer;
    }

    private void setupEnemyIntent(Label descLabel, VisualElement descContainer, EnemyInstance enemyInstance) {
        if(enemyInstance.currentIntent == null) {
            descLabel.text = "Preparing...";
        } else {
            descLabel.text = enemyInstance.currentIntent.GetDisplayValue().ToString();
            descLabel.AddToClassList("pillar-enemy-intent-text");
            var intentImage = new VisualElement();
            intentImage.AddToClassList("enemy-intent-image");
            intentImage.style.backgroundImage = new StyleBackground(viewDelegate.GetEnemyIntentImage(enemyInstance.currentIntent.intentType));
            descContainer.Add(intentImage);
        }
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
        healthLabel.AddToClassList("pillar-tab-text");
        healthLabel.text = stats.getCurrentHealth().ToString();
        healthTab.Add(healthLabel);
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
        if (combatInstance) {
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

        drawButton.clicked += DrawButtonOnClick;
        discardButton.clicked += DiscardButtonOnClick;

        drawerContainer.Add(drawButton);
        drawerContainer.Add(discardButton);

        drawerContainer.style.visibility = Visibility.Hidden;

        return drawerContainer;
    }

    private void EntityOnPointerClick(ClickEvent evt) {
        Targetable targetable = uiEntity.GetTargetable();
        if (targetable == null) return;
        targetable.OnPointerClickUI(evt);
    }

    private void EntityOnPointerEnter(PointerEnterEvent evt) {
        Targetable targetable = uiEntity.GetTargetable();
        if (targetable == null) return;
        targetable.OnPointerEnterUI(evt);
    }

    private void EntityOnPointerLeave(PointerLeaveEvent evt) {
        Targetable targetable = uiEntity.GetTargetable();
        if (targetable == null) return;
        targetable.OnPointerLeaveUI(evt);
    }

    private void DrawButtonOnClick() {
        Debug.Log("Draw button clicked");
        DeckInstance deckInstance = uiEntity.GetDeckInstance();
        if (deckInstance == null) {
            Debug.LogError("Entity " + uiEntity.GetName() + " does not have a deck instance, which is crazy, because it's clearly a companion");
            return;
        }
        viewDelegate.InstantiateCardView(deckInstance.GetShuffledDrawPile(), deckInstance.combatInstance.name + " draw pile");
}

    private void DiscardButtonOnClick() {
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

    private Tuple<int, int> GetWidthAndHeight() {
        int width = (int)(Screen.width * SCREEN_WIDTH_PERCENT);
        int height = (int)(width * RATIO);

        // This drove me insane btw
        #if UNITY_EDITOR
        UnityEditor.PlayModeWindow.GetRenderingResolution(out uint windowWidth, out uint windowHeight);
        width = (int)(windowWidth * SCREEN_WIDTH_PERCENT);
        height = (int)(width * RATIO);
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
        GameObject tweenTarget = new GameObject("ScaleBumpTarget");

        if (LeanTween.isTweening(tweenTarget)) {
            // return here if we want new tweens to just not start until old ones are done
            // return;

            // call the following code if we instead want to rest the entity to its base state and start the new tween
            if (combatInstance == null || combatInstance.GetComponent<Transform>() == null) {
                combatInstance.GetComponent<Transform>().localScale = originalScale;
            }
            LeanTween.cancel(tweenTarget);
            GameObject.Destroy(tweenTarget);
            combatInstance.GetComponent<Transform>().localScale = originalScale;
            entityContainer.style.scale = new StyleScale(new Scale(originalElementScale));
        }
 
        Transform combatInstanceTransform = combatInstance.GetComponent<Transform>();

        originalScale = combatInstanceTransform.localScale;

        originalElementScale = new Vector2(
            entityContainer.style.scale.value.value.x,
            entityContainer.style.scale.value.value.y
        );

        VisualElement companionContainer = entityContainer.Q<VisualElement>(className: "portrait-container");

        float duration = 0.125f;  // Total duration for the scale animation
        float minScale = (float)Math.Min(.75, .9 - scale / 500);  // scale bump increases in intensity if entity takes more damage (haven't extensively tested this)
        
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
                GameObject.Destroy(tweenTarget);
            });
    }
}