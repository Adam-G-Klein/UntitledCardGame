using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
    private List<EnemyView> entityViews = new List<EnemyView>();
    private List<CompanionView> companionViews = new List<CompanionView>();


    [SerializeField]
    private GameObject cardViewUIPrefab;
    [SerializeField]
    private GameObject newCardViewUIPrefab;
    private bool inMenu = false;
    private bool inDeckView = false;
    private bool combatOver = false;
    public MapView mapView;
    
    private Dictionary<CombatInstance, CompanionView> combatInstanceToCompanionView;
    private Dictionary<CombatInstance, EnemyView> combatInstanceToEnemyView;

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
        setupEnemies(root.Q<VisualElement>("enemyContainer"), enemies.Cast<IUIEntity>());
        SetupCompanions(root.Q<VisualElement>("companionContainer"), companions);
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Ignore);
        
        VisualElement endTurnElement = root.Q<VisualElement>("end-turn");
        endTurnElement.pickingMode = PickingMode.Position;
        VisualElementUtils.RegisterSelected(endTurnElement, EndPlayerTurnHandler);
        IconButton endTurnButton = endTurnElement as IconButton;
        endTurnButton.SetIcon(GFGInputAction.END_TURN, ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.END_TURN));
        ControlsManager.Instance.RegisterIconChanger(endTurnButton);

        setupComplete = true;
        VisualElement mapRoot = root.Q("mapRoot");
        mapRoot.Clear();
        mapView = new MapView(enemyEncounterManager);
        mapView.mapContainer.Q<Label>("money-indicator-label").text = gameState.playerData.GetValue().gold.ToString() + "G";
        mapRoot.Add(mapView.mapContainer);

        IconButton deckViewButton = root.Q<IconButton>("deck-view-button");
        deckViewButton.RegisterOnSelected(ShowDeckView);
        FocusManager.Instance.RegisterFocusableTarget(deckViewButton.AsFocusable());
        deckViewButton.SetIcon(
            GFGInputAction.OPEN_MULTI_DECK_VIEW,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.OPEN_MULTI_DECK_VIEW));
        ControlsManager.Instance.RegisterIconChanger(deckViewButton);
        deckViewButton.pickingMode = PickingMode.Position; // doesn't seem like this is being respected :( not sure why...
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
        this.combatInstanceToCompanionView = new();
        this.combatInstanceToEnemyView = new();
        SetupEnemies(enemyContainer, enemies);
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
            foreach (EnemyView entityView in entityViews) {
                entityView.UpdateView();
            }
            foreach (CompanionView view in companionViews) {
                view.UpdateView();
            }
            foreach (IUIEventReceiver view in pickingModePositionList) {
                view.SetPickingModes(!inMenu && !inDeckView && !combatOver);
            }
        }
    }

    public void DisableFocusing() {
        foreach (EnemyView entityView in entityViews) {
            FocusManager.Instance.UnregisterFocusableTarget(entityView.container.GetUserData<VisualElementFocusable>());
        }
    }

    void Update() { }

    // This function runs the first frame, which creates the enemy views before the enemy instances
    // exist.
    private void setupEnemies(VisualElement container, IEnumerable<IUIEntity> entities) {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in entities) {
            container.Add(setupEnemy(entity, index).container);
            index++;
        }
    }

    private void SetupEnemies(VisualElement container, IEnumerable<EnemyInstance> enemyInstances) {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (EnemyInstance entity in enemyInstances) {
            EnemyView enemyView = setupEnemy(entity, index);
            entity.enemyView = enemyView;
            container.Add(enemyView.container);
            combatInstanceToEnemyView.Add(entity.combatInstance, enemyView);
            index++;
        }
    }

    // This function runs first frame, which creates the companion views before the companion instances
    // exist.
    private void SetupCompanions(VisualElement container, IEnumerable<IUIEntity> companions) {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in companions) {
            container.Insert(0, SetupCompanion(entity, index).container);
            index++;
        }
    }

    // This function is run every time after the first time the companion views are created, so that the companion instances
    // are created.
    private void SetupCompanions(VisualElement container, IEnumerable<CompanionInstance> companionInstances)
    {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (CompanionInstance entity in companionInstances)
        {
            CompanionView companionView = SetupCompanion(entity, index);
            // This is why this is a separate function
            // this bridge is a nice way to get from PlayableCard->DeckInstance->CompanionInstance->CompanionView
            entity.companionView = companionView;
            // Need to put them in reverse order due to some UI layering issues with max health indicator
            container.Insert(0, companionView.container);
            combatInstanceToCompanionView.Add(entity.combatInstance, companionView);
            index++;
        }
    }

    private CompanionView SetupCompanion(IUIEntity companionInstance, int index) {
        CompanionView companionView = new CompanionView(
                companionInstance,
                this.enemyEncounterManager.encounterConstants.companionViewTemplate,
                index,
                CompanionView.COMBAT_CONTEXT,
                this);

        pickingModePositionList.Add(companionView);
        companionViews.Add(companionView);
        
        VisualElementFocusable focusable = companionView.container.GetUserData<VisualElementFocusable>();
        focusable.SetTargetType(Targetable.TargetType.Companion);
        FocusManager.Instance.RegisterFocusableTarget(focusable);

        return companionView;
    }

    private EnemyView setupEnemy(IUIEntity entity, int index) {
        EnemyView newEntityView = new EnemyView(entity, index, this);
        pickingModePositionList.Add(newEntityView);
        entityViews.Add(newEntityView);

        VisualElementFocusable entityViewFocusable = newEntityView.container.GetUserData<VisualElementFocusable>();
        entityViewFocusable.SetTargetType(Targetable.TargetType.Enemy);
        FocusManager.Instance.RegisterFocusableTarget(entityViewFocusable);

        return newEntityView;
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

    private void EndPlayerTurnHandler()
    {
        enemyEncounterManager.TryEndPlayerTurn();
    }

    public void ShowDeckView(ClickEvent evt)
    {
        MultiDeckViewManager.Instance.ShowCombatDeckView();
        Debug.LogError("trying to show deck view");
    }

    public void DamageIndicator(CombatInstance instance, int damage) {
        // Purely a visual thing, so would rather not break the game if this fails
        try {
            VisualElement originVE;
            if (combatInstanceToCompanionView.ContainsKey(instance)) {
                originVE = combatInstanceToCompanionView[instance].container;
            } else {
                originVE = combatInstanceToEnemyView[instance].container;
            }

            float randomX = UnityEngine.Random.Range(originVE.worldBound.xMin + (originVE.layout.width / 3f), originVE.worldBound.xMax - (originVE.layout.width / 3f));
            // Weird values in the division here just force the location to be towards the center of the enemy sprite
            float randomY = UnityEngine.Random.Range(originVE.worldBound.yMin + (originVE.layout.height / 6f), originVE.worldBound.yMax - (originVE.layout.height / 2f));

            Label damageLabel = new Label();
            damageLabel.AddToClassList("damage-indicator-label");
            damageLabel.text = damage.ToString();
            root.Add(damageLabel);
            // damageLabel.style.left = randomX - (damageLabel.layout.width / 2f);
            // damageLabel.style.top = randomY - (damageLabel.layout.height / 2f);
            damageLabel.style.left = randomX;
            damageLabel.style.top = randomY;

            float damageIndicatorTime = 1f;
            
            // Scale
            LeanTween.value(6f, 0.2f, (damageIndicatorTime) / 2)
                .setEase(LeanTweenType.easeInExpo)
                .setOnUpdate((float value) => {
                    damageLabel.transform.scale = new Vector3(value, value, 1f);
                });
            
            // Position 
            LeanTween.value(0f, 1f, damageIndicatorTime)
                .setEase(LeanTweenType.linear)
                .setOnUpdate((float value) => {
                    // damageLabel.style.top = randomY + (value * damageLabel.style.height.value.value);
                })
                .setOnComplete(() => {
                    root.Remove(damageLabel);
                });

        } catch (Exception e) {
            Debug.LogWarning(e);
        }
    }
}
