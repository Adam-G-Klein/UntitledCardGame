using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;

public class CombatEncounterView : GenericSingleton<CombatEncounterView>
{
    public GameStateVariableSO gameState;
    private VisualElement root;

    UIDocumentScreenspace docRenderer;

    public static string DETAILS_CONTAINER_SUFFIX = "-details";
    public static string DETAILS_HEADER_SUFFIX = "-details-title";
    public static string DETAILS_DESCRIPTION_SUFFIX = "-details-desc";

    [Header("Needs its own reference because the singleton isn't alive in time")]
    public GameplayConstantsSO gameplayConstants;
    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";
    public static string STATUS_EFFECTS_TAB_CLASSNAME = "status-effect";
    public static string STATUS_EFFECTS_IMAGE_CLASSNAME = "status-effect-image";
    public static string STATUS_EFFECTS_TEXT_CLASSNAME = "pillar-tab-text";
    public static string HEALTH_TAB_SUFFIX = "-health-tab";

    private bool setupComplete = false;

    [SerializeField]
    private StatusEffectsSO statusEffectsSO;
    [SerializeField]
    private EnemyIntentsSO enemyIntentsSO;

    private List<VisualElement> pickingModePositionList = new List<VisualElement>();
    [SerializeField]
    private float pulseAnimationSpeed = 0.1f;
    public bool animateHearts = false;

    private List<IEnumerator> pulseAnimations = new List<IEnumerator>();

    [SerializeField]
    private GameObject cardViewUIPrefab;
    private bool inTooltip = false;

    public void SetupFromGamestate()
    {
        docRenderer = gameObject.GetComponent<UIDocumentScreenspace>();
        root = GetComponent<UIDocument>().rootVisualElement;
        if(!statusEffectsSO) {
            Debug.LogError("StatusEffectsSO is null, Go to ScriptableObjects/Configuration and set the SO there in the CombatCanvasUIDocument/CombatUI prefab");
            return;
        }
        if(!enemyIntentsSO) {
            Debug.LogError("enemyIntentsSO is null, Go to ScriptableObjects/Configuration and set the SO there in the CombatCanvasUIDocument/CombatUI prefab");
            return;
        }
        if(!gameState.activeEncounter.GetValue().GetType().Equals(typeof(EnemyEncounter))) {
            Debug.LogError("Active encounter is not an EnemyEncounter, Go to ScriptableObjects/Variables/GameState.so and hit Set Active Encounter to set the encounter to an enemy encounter");
            return;
        }
        setupCardSlots();
        List<Enemy> enemies = ((EnemyEncounter)gameState.activeEncounter.GetValue()).enemyList;
        List<Companion> companions = gameState.companions.activeCompanions;
        setupEntities(root.Q<VisualElement>("enemyContainer"), enemies.Cast<IUIEntity>(), true);
        setupEntities(root.Q<VisualElement>("companionContainer"), companions.Cast<IUIEntity>(), false);
        root.Q<Label>("money").text = gameState.playerData.GetValue().gold.ToString();
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Ignore);
        setupComplete = true;
    }

    public void UpdateView() {
        if(!setupComplete) {
            SetupFromGamestate();
        } else {
            if(animateHearts) {
                foreach(IEnumerator anim in pulseAnimations) {
                    StopCoroutine(anim);
                }
                pulseAnimations.Clear();
            }
            VisualElement enemyContainer = root.Q<VisualElement>("enemyContainer");
            VisualElement companionContainer = root.Q<VisualElement>("companionContainer");
            enemyContainer.Clear();
            companionContainer.Clear();
            pickingModePositionList.Clear();
            List<CompanionInstance> companions = EnemyEncounterViewModel.Instance.companions;
            List<EnemyInstance> enemies = EnemyEncounterViewModel.Instance.enemies;
            setupEntities(root.Q<VisualElement>("enemyContainer"), enemies.Cast<IUIEntity>(), true);
            setupEntities(root.Q<VisualElement>("companionContainer"), companions.Cast<IUIEntity>(), false);
            root.Q<Label>("money").text = gameState.playerData.GetValue().gold.ToString();
            UIDocumentUtils.SetAllPickingMode(enemyContainer, PickingMode.Ignore);
            UIDocumentUtils.SetAllPickingMode(companionContainer, PickingMode.Ignore);
            foreach (VisualElement ve in pickingModePositionList) {
                ve.pickingMode = PickingMode.Position;
            }
        }
        docRenderer.SetStateDirty();
    }

    void Update()
    {
        /*
        foreach(KeyValuePair<IUIEntity, VisualElement> kvp in animatedHealthTabs) {
            pulseAnimateStep(kvp.Value);
        }
        */
    }

    private void setupEntities(VisualElement container, IEnumerable<IUIEntity> entities, bool isEnemy) {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in entities) {
            container.Add(setupEntity(entity, index, isEnemy));
            index++;
        }
    }

    private VisualElement setupEntity(IUIEntity entity, int index, bool isEnemy) {
        var pillar = new VisualElement();
        pillar.AddToClassList("entity-pillar");

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

        // it's important to add the hover detector *before* the drawer,
        // or it will pick away the click events from the buttons
        VisualElement hoverDetector = new VisualElement();
        hoverDetector.AddToClassList("pillar-hover-box");
        pickingModePositionList.Add(hoverDetector);
        pillar.Add(hoverDetector);
        registerModelUpdateOnHovers(entity, hoverDetector);

        pillar.Add(pillarContainer);

        if(!isEnemy)
            pillar.Add(setupCardDrawer(entity));

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
        VisualElement descriptionContainer = setupCardColumnDescription(entity, detailsContainer, index, isEnemy);
        registerModelUpdateOnHovers(entity, descriptionContainer);

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
        detailsContainer.AddToClassList("pillar-details");

        var titleContainer = new VisualElement();
        titleContainer.AddToClassList("pillar-name");
        if (isEnemy) {
            titleContainer.AddToClassList("enemy-pillar-name");
        }
        var titleLabel = new Label();
        titleLabel.AddToClassList("pillar-name");
        if (isEnemy) {
            titleLabel.AddToClassList("enemy-pillar-name");
        }
        titleContainer.Add(titleLabel);
        titleLabel.text = entity.GetName();
        detailsContainer.Add(titleContainer);
        return detailsContainer;
    }

    // returns the description container, which holds the enemy intent, the companion description, and the
    // deck drawers on hover for companions
    private VisualElement setupCardColumnDescription(IUIEntity entity, VisualElement detailsContainer, int index, bool isEnemy) {
        var descContainer = new VisualElement();
        descContainer.AddToClassList("pillar-text");
        pickingModePositionList.Add(descContainer);


        var descLabel = new Label();

        EnemyInstance enemyInstance = entity.GetEnemyInstance();
        if(enemyInstance) {
            VisualElement innerContainer = new VisualElement();
            innerContainer.AddToClassList("enemy-intent-inner-container");
            setupEnemyIntent(descLabel, innerContainer, enemyInstance);
            innerContainer.Add(descLabel);
            descContainer.Add(innerContainer);
        } else { // then we know it's a companion
            descLabel.AddToClassList("pillar-desc-label");
            descLabel.text = entity.GetDescription();
            descContainer.Add(descLabel);
        }
        return descContainer;
    }

    private void setupEnemyIntent(Label descLabel, VisualElement descContainer, EnemyInstance enemyInstance) {
        if(enemyInstance.currentIntent == null) {
            descLabel.text = "Preparing...";
        } else {
            descLabel.text = enemyInstance.currentIntent.GetDisplayValue().ToString();
            descLabel.AddToClassList("pillar-enemy-intent-text");
            var intentImage = new VisualElement();
            intentImage.AddToClassList("enemy-intent-image");
            intentImage.style.backgroundImage = new StyleBackground(enemyIntentsSO.GetIntentImage(enemyInstance.currentIntent.intentType));
            descContainer.Add(intentImage);
        }
    }

    private VisualElement setupCardDrawer(IUIEntity entity) {
        VisualElement drawerContainer = new VisualElement();
        drawerContainer.AddToClassList("pillar-drawer-menu");

        UnityEngine.UIElements.Button drawButton = new UnityEngine.UIElements.Button();
        drawButton.AddToClassList("drawer-button");
        drawButton.text = "Draw";


        UnityEngine.UIElements.Button discardButton = new UnityEngine.UIElements.Button();
        discardButton.AddToClassList("drawer-button");
        discardButton.text = "Discard";

        if(EnemyEncounterViewModel.Instance.hoveredEntity != entity) {
            drawerContainer.style.display = DisplayStyle.None;
        }

        pickingModePositionList.Add(drawButton);
        pickingModePositionList.Add(discardButton);
        pickingModePositionList.Add(drawerContainer);

        // this should SO be somewhere else but im ngl I kinda just feel like sending it rn
        drawButton.RegisterCallback<ClickEvent>(evt => {
            HideDrawer(entity);
            Debug.Log("Draw button clicked");
            DeckInstance deckInstance = entity.GetDeckInstance();
            if(deckInstance == null) {
                Debug.LogError("Entity " + entity.GetName() + " does not have a deck instance, which is crazy, because it's clearly a companion");
                return;
            }
            GameObject gameObject = GameObject.Instantiate(
                cardViewUIPrefab,
                Vector3.zero,
                Quaternion.identity);
            CardViewUI cardViewUI = gameObject.GetComponent<CardViewUI>();
            cardViewUI.Setup(deckInstance.GetShuffledDrawPile(),
                0,
                deckInstance.combatInstance.name + " draw pile",
                0);
        });

        // this should SO be somewhere else but im ngl I kinda just feel like sending it rn
        discardButton.RegisterCallback<ClickEvent>(evt => {
            HideDrawer(entity);
            Debug.Log("Discard button clicked");
            DeckInstance deckInstance = entity.GetDeckInstance();
            if(deckInstance == null) {
                Debug.LogError("Entity " + entity.GetName() + " does not have a deck instance, which is crazy, because it's clearly a companion");
                return;
            }
            GameObject gameObject = GameObject.Instantiate(
                cardViewUIPrefab,
                Vector3.zero,
                Quaternion.identity);
            CardViewUI cardViewUI = gameObject.GetComponent<CardViewUI>();
            cardViewUI.Setup(deckInstance.GetShuffledDiscardPile(),
                0,
                deckInstance.combatInstance.name + " discard pile",
                0);
        });
        drawerContainer.Add(drawButton);
        drawerContainer.Add(discardButton);

        return drawerContainer;
    }

    private VisualElement setupHealthAndBlockTabs(IUIEntity entityInstance) {
        var tabContainer = new VisualElement();
        tabContainer.AddToClassList("pillar-tab-container");
        CombatStats stats = entityInstance.GetCombatStats();

        // Do an animate
        var healthTab = new VisualElement();
        healthTab.AddToClassList("health-tab");
        healthTab.AddToClassList("animate-grow");
        if(animateHearts) {
            IEnumerator anim = pulseAnimation(healthTab);
            pulseAnimations.Add(anim);
            StartCoroutine(anim);
        }
        var healthLabel = new Label();
        healthLabel.AddToClassList("pillar-tab-text");
        healthLabel.text = stats.getCurrentHealth().ToString();
        healthTab.Add(healthLabel);
        tabContainer.Add(healthTab);

        CombatInstance combatInstance = entityInstance.GetCombatInstance();

        if(combatInstance && combatInstance.GetStatus(StatusEffectType.Defended) > 0) {
            var blockContainer = new VisualElement();
            blockContainer.AddToClassList("block-tab");
            var blockLabel = new Label();
            blockLabel.AddToClassList("pillar-tab-text");
            blockLabel.text = combatInstance.GetStatus(StatusEffectType.Defended).ToString();
            blockContainer.Add(blockLabel);
            tabContainer.Add(blockContainer);
        }

        return tabContainer;
    }

    private VisualElement setupStatusEffectsTabs(IUIEntity entityInstance) {

        var tabContainer = new VisualElement();
        tabContainer.AddToClassList("pillar-tab-container");

        CombatInstance combatInstance = entityInstance.GetCombatInstance();
        if(combatInstance) {
            foreach (KeyValuePair<StatusEffectType, int> kvp in combatInstance.GetDisplayedStatusEffects()) {
                var statusEffectTab = new VisualElement();
                statusEffectTab.AddToClassList("status-tab");
                Sprite sprite = statusEffectsSO.GetStatusEffectImage(kvp.Key);
                if(sprite == null) {
                    Debug.LogError("Can't display status, StatusEffectSO does not have an image for " + kvp.Key.ToString());
                    continue;
                }
                statusEffectTab.style.backgroundImage = new StyleBackground(sprite.texture);
                var statusEffectText = new Label();
                statusEffectText.text = kvp.Value.ToString();
                statusEffectText.AddToClassList("status-tab-text");
                statusEffectTab.Add(statusEffectText);
                tabContainer.Add(statusEffectTab);
            }
        }

        return tabContainer;

    }

    private void setupCardSlots()
    {
        var container = root.Q<VisualElement>("cardContainer");
        int index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        int maxHandSize = gameplayConstants.MAX_HAND_SIZE;
        for (int i = 0; i < maxHandSize; i++) {
            VisualElement cardContainer = new VisualElement();
            cardContainer.AddToClassList("companion-card-placer");
            container.Add(cardContainer);
            var card = new VisualElement();
            card.name = UIDocumentGameObjectPlacer.CARD_UIDOC_ELEMENT_PREFIX + index;
            card.AddToClassList("cardPlace");
            cardContainer.Add(card);
            index++;
        }
    }

    private IEnumerator pulseAnimation(VisualElement pulseElement) {
        while (true) {
            pulseAnimateStep(pulseElement);
            yield return new WaitForEndOfFrame();
        }
    }

    public void pulseAnimateStep(VisualElement pulseElement) {
        // Make the heart bounce between scale of 1 and 2
        bool growing = pulseElement.ClassListContains("animate-grow");
        Vector2 currentScale = pulseElement.transform.scale;
        if (growing) {
            currentScale.x += pulseAnimationSpeed * Time.deltaTime;
            currentScale.y += pulseAnimationSpeed * Time.deltaTime;
            if(currentScale.x >= 2) {
                pulseElement.RemoveFromClassList("animate-grow");
                pulseElement.AddToClassList("animate-shrink");
            }
        } else {
            currentScale.x -= pulseAnimationSpeed * Time.deltaTime;
            currentScale.y -= pulseAnimationSpeed * Time.deltaTime;
            if(currentScale.x <= 1) {
                pulseElement.RemoveFromClassList("animate-shrink");
                pulseElement.AddToClassList("animate-grow");
            }
        }
        pulseElement.transform.scale = currentScale;
    }

    public void updateMana(int mana) {
        root.Q<Label>("manaCounter").text = mana.ToString();
        docRenderer.SetStateDirty();
    }

    public void updateMoney(int money) {
        docRenderer.SetStateDirty();
    }
    
    // Need to check the entity because the visual element is re-created on every frame
    // if you try to compare the ve then they will never be equal, different object hash codes
    private void registerModelUpdateOnHovers(IUIEntity entity, VisualElement ve) {
        ve.RegisterCallback<MouseEnterEvent>(evt => {
            if(EnemyEncounterViewModel.Instance.hoveredEntity != entity && !InTooltip() && !CombatOver()) {
                Debug.Log("Hovering over " + entity.GetName());
                EnemyEncounterViewModel.Instance.hoveredElement = ve;
                EnemyEncounterViewModel.Instance.hoveredEntity = entity;
                EnemyEncounterViewModel.Instance.SetStateDirty();
            }
        });
        ve.RegisterCallback<MouseLeaveEvent>(evt => {
            HideDrawer(entity);
        });
    }

    private void HideDrawer(IUIEntity entity) {
        if(EnemyEncounterViewModel.Instance.hoveredEntity == entity) {
            EnemyEncounterViewModel.Instance.hoveredElement = null;
            EnemyEncounterViewModel.Instance.hoveredEntity = null;
            EnemyEncounterViewModel.Instance.SetStateDirty();
        }
    }
    private bool InTooltip() {
        return EnemyEncounterManager.Instance.GetInToolTip();
    }

    private bool isHovered(IUIEntity entity) {
        return EnemyEncounterViewModel.Instance.hoveredEntity == entity;
    }
    
    private bool CombatOver() {
        return EnemyEncounterManager.Instance.GetCombatOver();
    }
}
