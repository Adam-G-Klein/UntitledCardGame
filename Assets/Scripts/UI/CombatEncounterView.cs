using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;

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

    public void SetupFromGamestate() 
    {
        docRenderer = gameObject.GetComponent<UIDocumentScreenspace>();
        root = GetComponent<UIDocument>().rootVisualElement;
        if(!statusEffectsSO) {
            Debug.LogError("StatusEffectsSO is null, Go to ScriptableObjects/Configuration and set the SO there in the CombatCanvasUIDocument/CombatUI prefab");
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
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Ignore);
        setupComplete = true;
    }

    public void UpdateView() {
        if(!setupComplete) {
            SetupFromGamestate();
        } else {
            VisualElement enemyContainer = root.Q<VisualElement>("enemyContainer");
            VisualElement companionContainer = root.Q<VisualElement>("companionContainer");
            enemyContainer.Clear();
            companionContainer.Clear();
            List<CompanionInstance> companions = EnemyEncounterViewModel.Instance.companions;
            List<EnemyInstance> enemies = EnemyEncounterViewModel.Instance.enemies;
            setupEntities(root.Q<VisualElement>("enemyContainer"), enemies.Cast<IUIEntity>(), true);
            setupEntities(root.Q<VisualElement>("companionContainer"), companions.Cast<IUIEntity>(), false);
            UIDocumentUtils.SetAllPickingMode(enemyContainer, PickingMode.Ignore);
            UIDocumentUtils.SetAllPickingMode(companionContainer, PickingMode.Ignore);
        }
    }

    private void setupEntities(VisualElement container, IEnumerable<IUIEntity> entities, bool isEnemy) {
        // DAE use enh for loops and keep track of index like this cause they hate array syntax
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
        
        return pillar;
    }

    private VisualElement setupHealthAndBlockTabs(IUIEntity entityInstance) {
        var tabContainer = new VisualElement();
        tabContainer.AddToClassList("pillar-tab-container");
        CombatStats stats = entityInstance.GetCombatStats();

        var healthTab = new VisualElement();
        healthTab.AddToClassList("health-tab");
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

    private VisualElement setupCardColumn(IUIEntity entity, int index, bool isEnemy) {
        var column = new VisualElement();
        column.name = entity.GetName();
        column.AddToClassList("pillar-card-column");

        var portraitContainer = new VisualElement();
        var baseString = isEnemy ? UIDocumentGameObjectPlacer.ENEMY_UIDOC_ELEMENT_PREFIX : UIDocumentGameObjectPlacer.COMPANION_UIDOC_ELEMENT_PREFIX;
        string portraitContainerName = baseString + index.ToString();
        portraitContainer.name = portraitContainerName;
        portraitContainer.AddToClassList("portrait-container");
        column.Add(portraitContainer);
        column.AddToClassList(portraitContainer.name + STATUS_EFFECTS_CONTAINER_SUFFIX);

        var detailsContainer = new VisualElement();
        // TODO: figure out how to avoid querying from root. All the elements we want to query need to have 
        // unique names from root for now. 
        detailsContainer.AddToClassList(portraitContainer.name + DETAILS_CONTAINER_SUFFIX);
        detailsContainer.AddToClassList("pillar-details");

        var titleContainer = new VisualElement();
        titleContainer.AddToClassList("pillar-name");
        var titleLabel = new Label();
        titleLabel.AddToClassList(portraitContainer.name + DETAILS_HEADER_SUFFIX);
        titleLabel.AddToClassList("pillar-name");
        titleContainer.Add(titleLabel);
        detailsContainer.Add(titleContainer);
        var descContainer = new VisualElement();
        descContainer.AddToClassList("pillar-text");
        var descLabel = new Label();
        descLabel.AddToClassList(portraitContainer.name + DETAILS_DESCRIPTION_SUFFIX);
        descLabel.AddToClassList("pillar-desc-label");
        descContainer.Add(descLabel);
        titleLabel.text = entity.GetName(); 

        detailsContainer.Add(descContainer);
        EnemyInstance enemyInstance = entity.GetEnemyInstance();
        if(enemyInstance && enemyInstance.currentIntent != null) {
            descLabel.text = enemyInstance.currentIntent.intentType.ToString();
        } else {
            descLabel.text = entity.GetDescription();
        }

        column.Add(detailsContainer);

        return column;
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

    public void updateMana(int mana) {
        root.Q<Label>("manaCounter").text = mana.ToString();
        docRenderer.SetStateDirty();
    }
}
