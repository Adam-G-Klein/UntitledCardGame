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
    private EnemyEncounterManager enemyEncounterManager;

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
    private List<CompanionView> companionViews = new List<CompanionView>();


    [SerializeField]
    private GameObject cardViewUIPrefab;
    [SerializeField]
    private GameObject newCardViewUIPrefab;
    private bool inMenu = false;
    private bool inDeckView = false;
    private bool combatOver = false;

    public void SetupFromGamestate(EnemyEncounterManager enemyEncounterManager)
    {
        this.enemyEncounterManager = enemyEncounterManager;
        docRenderer = gameObject.GetComponent<UIDocumentScreenspace>();
        root = GetComponent<UIDocument>().rootVisualElement;
        if (!statusEffectsSO)
        {
            Debug.LogError("StatusEffectsSO is null, Go to ScriptableObjects/Configuration and set the SO there in the CombatCanvasUIDocument/CombatUI prefab");
            return;
        }
        if (!enemyIntentsSO)
        {
            Debug.LogError("enemyIntentsSO is null, Go to ScriptableObjects/Configuration and set the SO there in the CombatCanvasUIDocument/CombatUI prefab");
            return;
        }
        if (!gameState.activeEncounter.GetValue().GetType().Equals(typeof(EnemyEncounter)))
        {
            Debug.LogError("Active encounter is not an EnemyEncounter, Go to ScriptableObjects/Variables/GameState.so and hit Set Active Encounter to set the encounter to an enemy encounter");
            return;
        }
        List<Enemy> enemies = ((EnemyEncounter)gameState.activeEncounter.GetValue()).enemyList;
        List<Companion> companions = gameState.companions.activeCompanions;
        setupEntities(root.Q<VisualElement>("enemyContainer"), enemies.Cast<IUIEntity>(), true);
        // setupEntities(root.Q<VisualElement>("companionContainer"), companions.Cast<IUIEntity>(), false);
        SetupCompanions(root.Q<VisualElement>("companionContainer"), companions);
        //root.Q<Label>("money").text = gameState.playerData.GetValue().gold.ToString();
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Ignore);
        setupComplete = true;
        VisualElement mapRoot = root.Q("mapRoot");
        mapRoot.Clear();
        MapView mapView = new MapView(enemyEncounterManager);
        mapView.mapContainer.Q<Label>("money-indicator-label").text = gameState.playerData.GetValue().gold.ToString() + "G";
        mapRoot.Add(mapView.mapContainer);
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
        FocusManager.Instance.UnregisterFocusables(enemyContainer);
        FocusManager.Instance.UnregisterFocusables(companionContainer);
        enemyContainer.Clear();
        companionContainer.Clear();
        setupEntities(enemyContainer, enemies.Cast<IUIEntity>(), true);
        // setupEntities(companionContainer, companions.Cast<IUIEntity>(), false);
        SetupCompanions(companionContainer, companions);
        UIDocumentUtils.SetAllPickingMode(enemyContainer, PickingMode.Ignore);
        UIDocumentUtils.SetAllPickingMode(companionContainer, PickingMode.Ignore);
        FocusManager.Instance.RegisterFocusables(GetComponent<UIDocument>());
    }

    public void UpdateView() {
        if(!setupComplete) {
            SetupFromGamestate(this.enemyEncounterManager);
        } else {
            root.Q<Label>("money-indicator-label").text = gameState.playerData.GetValue().gold.ToString() + "G";
            foreach (EntityView entityView in entityViews) {
                entityView.UpdateView();
            }
            foreach (CompanionView view in companionViews) {
                view.UpdateView();
            }
            foreach (IUIEventReceiver view in pickingModePositionList) {
                view.SetPickingModes(!inMenu && !inDeckView && !combatOver);
            }
        }
        docRenderer.SetStateDirty();
    }

    public void DisableFocusing() {
        foreach (EntityView entityView in entityViews) {
            FocusManager.Instance.UnregisterFocusableTarget(entityView.entityContainer.GetUserData<VisualElementFocusable>());
        }
    }

    void Update() { }

    private void setupEntities(VisualElement container, IEnumerable<IUIEntity> entities, bool isEnemy) {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in entities) {
            container.Add(setupEntity(entity, index, isEnemy));
            index++;
        }
    }

    private void SetupCompanions(VisualElement container, IEnumerable<IUIEntity> companions) {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in companions) {
            container.Add(SetupCompanion(entity, index).container);
            index++;
        }
    }

    private void SetupCompanions(VisualElement container, IEnumerable<CompanionInstance> companionInstances)
    {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (CompanionInstance entity in companionInstances)
        {
            CompanionView companionView = SetupCompanion(entity, index);
            // This is why this is a separate function
            // this bridge is a nice way to get from PlayableCard->DeckInstance->CompanionInstance->CompanionView
            entity.companionView = companionView;
            container.Add(companionView.container);
            index++;
        }
    }

    private CompanionView SetupCompanion(IUIEntity companionInstance, int index) {
        CompanionView companionView = new CompanionView(
                companionInstance,
                this.enemyEncounterManager.encounterConstants.companionViewTemplate,
                index,
                CompanionViewType.COMBAT,
                this);

        pickingModePositionList.Add(companionView);
        companionViews.Add(companionView);
        
        VisualElementFocusable focusable = companionView.container.GetUserData<VisualElementFocusable>();
        focusable.SetTargetType(Targetable.TargetType.Companion);
        FocusManager.Instance.RegisterFocusableTarget(focusable);

        return companionView;
    }

    private VisualElement setupEntity(IUIEntity entity, int index, bool isEnemy) {
        EntityView newEntityView = new EntityView(entity, index, isEnemy, this);
        newEntityView.AddDrawDiscardOnHover();
        pickingModePositionList.Add(newEntityView);
        entityViews.Add(newEntityView);

        VisualElementFocusable entityViewFocusable = newEntityView.entityContainer.GetUserData<VisualElementFocusable>();
        entityViewFocusable.SetTargetType(isEnemy ? Targetable.TargetType.Enemy : Targetable.TargetType.Companion);
        FocusManager.Instance.RegisterFocusableTarget(entityViewFocusable);

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
                newCardViewUIPrefab,
                Vector3.zero,
                Quaternion.identity);
        CardSelectionView cardSelectionView = gameObject.GetComponent<CardSelectionView>();
        cardSelectionView.Setup(cardList, promptText);
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
