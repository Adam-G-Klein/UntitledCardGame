using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;

public class CombatEncounterView : MonoBehaviour,
    IEntityViewDelegate
{
    public GameStateVariableSO gameState;
    private VisualElement root;

    UIDocumentScreenspace docRenderer;

    [Header("Needs its own reference because the singleton isn't alive in time")]
    public GameplayConstantsSO gameplayConstants;
    public static string STATUS_EFFECTS_CONTAINER_SUFFIX = "-status-effects";

    private bool setupComplete = false;

    [SerializeField]
    private StatusEffectsSO statusEffectsSO;
    [SerializeField]
    private EnemyIntentsSO enemyIntentsSO;

    private List<IUIEventReceiver> pickingModePositionList = new List<IUIEventReceiver>();
    private List<EntityView> entityViews = new List<EntityView>();


    [SerializeField]
    private GameObject cardViewUIPrefab;
    private bool inMenu = false;
    private bool inDeckView = false;
    private bool combatOver = false;

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
        List<Enemy> enemies = ((EnemyEncounter)gameState.activeEncounter.GetValue()).enemyList;
        List<Companion> companions = gameState.companions.activeCompanions;
        setupEntities(root.Q<VisualElement>("enemyContainer"), enemies.Cast<IUIEntity>(), true);
        setupEntities(root.Q<VisualElement>("companionContainer"), companions.Cast<IUIEntity>(), false);
        root.Q<Label>("money").text = gameState.playerData.GetValue().gold.ToString();
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Ignore);
        setupComplete = true;
    }

    /*
        This needs to happen because we have a bit of a circular dependency. The _Intstance monobehaviors
        can't be created until the UI is setup, but the EntityViews need a reference to the _Instances,
        so we first setup the UI with just Companion and Enemy from gamestate, then we reset them to hold
        references to the _Instances cast to IUIEntity afterwards.
    */
    public void ResetEntities(List<CompanionInstance> companions, List<EnemyInstance> enemies) {
        VisualElement enemyContainer = root.Q<VisualElement>("enemyContainer");
        VisualElement companionContainer = root.Q<VisualElement>("companionContainer");
        enemyContainer.Clear();
        companionContainer.Clear();
        setupEntities(enemyContainer, enemies.Cast<IUIEntity>(), true);
        setupEntities(companionContainer, companions.Cast<IUIEntity>(), false);
        UIDocumentUtils.SetAllPickingMode(enemyContainer, PickingMode.Ignore);
        UIDocumentUtils.SetAllPickingMode(companionContainer, PickingMode.Ignore);
    }

    public void UpdateView() {
        if(!setupComplete) {
            SetupFromGamestate();
        } else {
            root.Q<Label>("money").text = gameState.playerData.GetValue().gold.ToString();
            foreach (EntityView entityView in entityViews) {
                entityView.UpdateView();
            }
            foreach (IUIEventReceiver view in pickingModePositionList) {
                view.SetPickingModes(!inMenu && !inDeckView && !combatOver);
            }
        }
        docRenderer.SetStateDirty();
    }

    void Update() { }

    private void setupEntities(VisualElement container, IEnumerable<IUIEntity> entities, bool isEnemy) {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in entities) {
            container.Add(setupEntity(entity, index, isEnemy));
            index++;
        }
    }

    private VisualElement setupEntity(IUIEntity entity, int index, bool isEnemy) {
        EntityView newEntityView = new EntityView(entity, index, isEnemy, this);
        newEntityView.AddDrawDiscardOnHover();
        pickingModePositionList.Add(newEntityView);
        entityViews.Add(newEntityView);
        return newEntityView.entityContainer;
    }

    public void updateMana(int mana) {
        root.Q<Label>("manaCounter").text = mana.ToString();
        docRenderer.SetStateDirty();
    }

    public void updateMoney(int money) {
        docRenderer.SetStateDirty();
    }

    public Sprite GetStatusEffectSprite(StatusEffectType statusEffectType)
    {
        return statusEffectsSO.GetStatusEffectImage(statusEffectType);
    }

    public void InstantiateCardView(List<Card> cardList, string promptText)
    {
        GameObject gameObject = GameObject.Instantiate(
                cardViewUIPrefab,
                Vector3.zero,
                Quaternion.identity);
            CardViewUI cardViewUI = gameObject.GetComponent<CardViewUI>();
            cardViewUI.Setup(cardList,
                0,
                promptText,
                0);
    }

    public Sprite GetEnemyIntentImage(EnemyIntentType enemyIntentType)
    {
        return enemyIntentsSO.GetIntentImage(enemyIntentType);
    }

    public MonoBehaviour GetMonoBehaviour() {
        return this;
    }

    public void SetInMenu(bool inMenu) {
        this.inMenu = inMenu;
        UpdateView();
    }
    public void SetInDeckView(bool inDeckView) {
        this.inDeckView = inDeckView;
        UpdateView();
    }

    public void SetEndCombat() {
        combatOver = true;
    }
}
