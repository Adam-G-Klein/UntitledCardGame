using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CombatEncounterView : MonoBehaviour,
    IEntityViewDelegate,
    ICompanionViewDelegate
{
    public GameStateVariableSO gameState;
    private UIDocument uiDoc;
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
    [SerializeField] private AnimationCurve damageIndicatorXCurve;
    [SerializeField] private float damageIndicatorXDistance;
    [SerializeField] private AnimationCurve damageIndicatorYCurve;
    [SerializeField] private float damageIndicatorYDistance;
    [SerializeField] private float damageIndicatorTime = 1f;
    [SerializeField] private Color startingDamangeIndicatorColor;

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
    private CardInHandSelectionView cardInHandSelectionView;

    private Dictionary<CombatInstance, CompanionView> combatInstanceToCompanionView;
    private Dictionary<CombatInstance, EnemyView> combatInstanceToEnemyView;
    private bool bossFight = false;

    public void SetupFromGamestate(EnemyEncounterManager enemyEncounterManager, bool skipMapSetup = false, bool skipEnemySetup = false)
    {
        this.enemyEncounterManager = enemyEncounterManager;
        docRenderer = gameObject.GetComponent<UIDocumentScreenspace>();
        uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;
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
        if (!skipEnemySetup)
        {
            setupEnemies(root.Q<VisualElement>("enemyContainer"), enemies.Cast<IUIEntity>());
        }
        SetupCompanions(root.Q<VisualElement>("companionContainer"), companions);
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Ignore);

        VisualElement endTurnElement = root.Q<VisualElement>("end-turn");
        endTurnElement.pickingMode = PickingMode.Position;
        VisualElementUtils.RegisterSelected(endTurnElement, EndPlayerTurnHandler);
        IconButton endTurnButton = endTurnElement as IconButton;
        endTurnButton.SetIcon(GFGInputAction.END_TURN, ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.END_TURN));
        ControlsManager.Instance.RegisterIconChanger(endTurnButton);

        setupComplete = true;
        // for the boss fight
        VisualElement mapRoot = root.Q("mapRoot");
        mapRoot.Clear();
        if (!skipMapSetup)
        {
            mapView = new MapView(enemyEncounterManager);
            mapView.mapContainer.Q<Label>("money-indicator-label").text = gameState.playerData.GetValue().gold.ToString() + "$";
            mapRoot.Add(mapView.mapContainer);
        }

        cardInHandSelectionView = new CardInHandSelectionView(uiDoc, root.Q<VisualElement>("card-in-hand-selection-view"));

        IconButton deckViewButton = root.Q<IconButton>("deck-view-button");
        deckViewButton.RegisterOnSelected(ShowDeckView);
        FocusManager.Instance.RegisterFocusableTarget(deckViewButton.AsFocusable());
        deckViewButton.SetIcon(
            GFGInputAction.OPEN_MULTI_DECK_VIEW,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.OPEN_MULTI_DECK_VIEW));
        ControlsManager.Instance.RegisterIconChanger(deckViewButton);
        deckViewButton.pickingMode = PickingMode.Position;
    }

    public void DestroyMapAndEnemyUI()
    {
    //    clearViews();
    //    SetupFromGamestate(enemyEncounterManager, true, true);
    }

    private void clearViews()
    {
        VisualElement enemyContainer = root.Q<VisualElement>("enemyContainer");
        VisualElement companionContainer = root.Q<VisualElement>("companionContainer");
        FocusManager.Instance.UnregisterFocusables(enemyContainer);
        FocusManager.Instance.UnregisterFocusables(companionContainer);
        enemyContainer.Clear();
        companionContainer.Clear();
        this.combatInstanceToCompanionView = new();
        this.combatInstanceToEnemyView = new();
    }

    /*
        This needs to happen because we have a bit of a circular dependency. The _Intstance monobehaviors
        can't be created until the UI is setup, but the EntityViews need a reference to the _Instances,
        so we first setup the UI with just Companion and Enemy from gamestate, then we reset them to hold
        references to the _Instances cast to IUIEntity afterwards.
    */
    public void ResetEntities(List<CompanionInstance> companions, List<EnemyInstance> enemies)
    {
        clearViews();
        VisualElement enemyContainer = root.Q<VisualElement>("enemyContainer");
        VisualElement companionContainer = root.Q<VisualElement>("companionContainer");
        SetupEnemies(enemyContainer, enemies);
        // setupEntities(companionContainer, companions.Cast<IUIEntity>(), false);
        SetupCompanions(companionContainer, companions);
        UIDocumentUtils.SetAllPickingMode(enemyContainer, PickingMode.Ignore);
        UIDocumentUtils.SetAllPickingMode(companionContainer, PickingMode.Ignore);
        FocusManager.Instance.RegisterFocusables(GetComponent<UIDocument>());
    }

    public void UpdateView()
    {
        if (!setupComplete)
        {
            SetupFromGamestate(this.enemyEncounterManager);
        } else {
            var moneyVisElem = root.Q<Label>("money-indicator-label"); 
            // TODO, nullptr if big boss fight cuz no money indicator
            if(moneyVisElem != null)
            {
                moneyVisElem.text = gameState.playerData.GetValue().gold.ToString() + "G";
            }
            foreach (EnemyView entityView in entityViews) {
                entityView.UpdateView();
            }
            foreach (CompanionView view in companionViews)
            {
                view.UpdateView();
            }
            foreach (IUIEventReceiver view in pickingModePositionList)
            {
                view.SetPickingModes(!inMenu && !inDeckView && !combatOver);
            }
        }
    }

    public void DisableFocusing()
    {
        foreach (EnemyView entityView in entityViews)
        {
            FocusManager.Instance.UnregisterFocusableTarget(entityView.container.GetUserData<VisualElementFocusable>());
        }
    }

    void Update() { }

    // This function runs the first frame, which creates the enemy views before the enemy instances
    // exist.
    private void setupEnemies(VisualElement container, IEnumerable<IUIEntity> entities)
    {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in entities) {
            container.Add(setupEnemy(entity, index).container);
            // for when we have more bosses or different enemy types that need special handling
            switch (entity.GetDisplayType())
            {
                case DisplayType.MEOTHRA:
                    bossFight = true;
                    break;
                default:
                    break;
            }
            index++;
        }
    }

    private void SetupEnemies(VisualElement container, IEnumerable<EnemyInstance> enemyInstances)
    {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (EnemyInstance entity in enemyInstances)
        {
            // setupEnemy adds to the list
            EnemyView enemyView = setupEnemy(entity, index);
            entity.enemyView = enemyView;
            container.Add(enemyView.container);
            combatInstanceToEnemyView.Add(entity.combatInstance, enemyView);
            index++;
        }
    }

    // This function runs first frame, which creates the companion views before the companion instances
    // exist.
    private void SetupCompanions(VisualElement container, List<Companion> companions)
    {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (Companion companion in companions)
        {
            container.Insert(0, SetupCompanion(companion, index).container);
            index++;
        }
    }

    // This function is run every time after the first time the companion views are created, so that the companion instances
    // are created.
    private void SetupCompanions(VisualElement container, List<CompanionInstance> companionInstances)
    {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (CompanionInstance companionInstance in companionInstances)
        {
            CompanionView companionView = SetupCompanion(companionInstance.companion, index, companionInstance);
            // This is why this is a separate function
            // this bridge is a nice way to get from PlayableCard->DeckInstance->CompanionInstance->CompanionView
            companionInstance.companionView = companionView;
            // Need to put them in reverse order due to some UI layering issues with max health indicator
            container.Insert(0, companionView.container);
            combatInstanceToCompanionView.Add(companionInstance.combatInstance, companionView);
            index++;
        }
    }

    private CompanionView SetupCompanion(Companion companion, int index, CompanionInstance companionInstance = null)
    {
        CompanionView companionView = new CompanionView(
                companion,
                this.enemyEncounterManager.encounterConstants.companionViewTemplate,
                index,
                CompanionView.COMBAT_CONTEXT,
                this,
                companionInstance);

        pickingModePositionList.Add(companionView);
        companionViews.Add(companionView);

        VisualElementFocusable focusable = companionView.container.GetUserData<VisualElementFocusable>();
        focusable.SetTargetType(Targetable.TargetType.Companion);
        FocusManager.Instance.RegisterFocusableTarget(focusable);

        return companionView;
    }

    private EnemyView setupEnemy(IUIEntity entity, int index)
    {
        EnemyView newEntityView = new EnemyView(entity, index, this);
        pickingModePositionList.Add(newEntityView);
        entityViews.Add(newEntityView);

        VisualElementFocusable entityViewFocusable = newEntityView.container.GetUserData<VisualElementFocusable>();
        entityViewFocusable.SetTargetType(Targetable.TargetType.Enemy);
        FocusManager.Instance.RegisterFocusableTarget(entityViewFocusable);

        return newEntityView;
    }

    public void updateMana(int mana)
    {
        root.Q<Label>("manaCounter").text = mana.ToString();
        docRenderer.SetStateDirty();
    }

    public void updateMoney(int money)
    {
        docRenderer.SetStateDirty();
    }

    public CardInHandSelectionView GetCardSelectionView()
    {
        return this.cardInHandSelectionView;
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

    public void SetInMenu(bool inMenu)
    {
        this.inMenu = inMenu;
        UpdateView();
    }

    public void SetInDeckView(bool inDeckView)
    {
        this.inDeckView = inDeckView;
        UpdateView();
    }

    public void SetEndCombat()
    {
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

    public void DamageIndicator(CombatInstance instance, int damage)
    {
        // Purely a visual thing, so would rather not break the game if this fails
        try
        {
            VisualElement originVE;
            if (combatInstanceToCompanionView.ContainsKey(instance))
            {
                originVE = combatInstanceToCompanionView[instance].container;
            }
            else
            {
                originVE = combatInstanceToEnemyView[instance].container;
            }

            float randomX = UnityEngine.Random.Range(originVE.worldBound.xMin + (originVE.layout.width / 3f), originVE.worldBound.xMax - (originVE.layout.width / 3f));
            // Weird values in the division here just force the location to be towards the center of the enemy sprite
            float randomY = UnityEngine.Random.Range(originVE.worldBound.yMin - (originVE.layout.height / 10f), originVE.worldBound.yMin);
            bool goLeft = (originVE.worldBound.xMin + originVE.layout.width / 2) > randomX;//UnityEngine.Random.Range(0, 2) == 0;

            Label damageLabel = new Label();
            damageLabel.AddToClassList("damage-indicator-label");
            damageLabel.text = damage.ToString();
            root.Add(damageLabel);
            // damageLabel.style.left = randomX - (damageLabel.layout.width / 2f);
            // damageLabel.style.top = randomY - (damageLabel.layout.height / 2f);
            damageLabel.style.left = randomX;
            damageLabel.style.top = randomY;

            // Scale
            LeanTween.value(6f, 0.2f, damageIndicatorTime)
                .setEase(LeanTweenType.easeInSine)
                .setOnUpdate((float value) =>
                {
                    damageLabel.transform.scale = new Vector3(value, value, 1f);
                });

            // X Position 
            LeanTween.value(0f, 1f, damageIndicatorTime)
                .setEase(damageIndicatorXCurve)
                .setOnUpdate((float value) =>
                {
                    damageLabel.style.left = randomX + (goLeft ? -1 : 1) * (value * damageIndicatorXDistance);
                })
                .setOnComplete(() =>
                {
                    root.Remove(damageLabel);
                });

            // YPos
            LeanTween.value(0f, 1f, damageIndicatorTime)
                .setEase(damageIndicatorYCurve)
                .setOnUpdate((float value) =>
                {
                    damageLabel.style.top = randomY + (value * damageIndicatorYDistance);
                });

            // Color (interested to see how this feels)
            LeanTween.value(0f, 1f, damageIndicatorTime)
                .setEase(LeanTweenType.easeOutCubic)
                .setOnUpdate((float value) =>
                {
                    Color lerpedColor = Color.Lerp(startingDamangeIndicatorColor, Color.white, value);
                    lerpedColor.a = 1;
                    damageLabel.style.color = new StyleColor(lerpedColor);
                });

        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public void ViewDeck(DeckViewType deckViewType, Companion companion = null, CompanionInstance companionInstance = null)
    {
        if (companionInstance == null)
        {
            Debug.LogError("CombatEncounterManager: ViewDeck delegate called for a companion, but CompanionInstance is null!");
            return;
        }

        int startingTab;
        switch (deckViewType)
        {
            case DeckViewType.Draw:
                startingTab = 0;
                break;

            case DeckViewType.Discard:
            default:
                startingTab = 1;
                break;
        }

        MultiDeckViewManager.Instance.ShowCombatDeckView(companionInstance, startingTab);
    }

    public void SetCompanionsAndEnemiesEnabled(bool enabled)
    {
        foreach (CompanionView view in companionViews)
        {
            view.SetPickingModes(enabled);
        }

        foreach (EnemyView view in entityViews)
        {
            view.SetPickingModes(enabled);
        }
    }

    public void DestroyAllTooltips()
    {
        foreach (CombatInstance instance in combatInstanceToCompanionView.Keys)
        {
            try
            {
                instance.GetComponent<TooltipOnHover>()?.OnPointerExitVoid();
            }
            catch (MissingReferenceException exception)
            {
                // If an enemy has died, we'll hit this case, but we don't want to fail outright
                Debug.LogException(exception);
            }
        }

        foreach (CombatInstance instance in combatInstanceToEnemyView.Keys)
        {
            try
            {
                instance.GetComponent<TooltipOnHover>()?.OnPointerExitVoid();
            }
            catch (MissingReferenceException exception)
            {
                // If an enemy has died, we'll hit this case, but we don't want to fail outright
                Debug.LogException(exception);
            }
        }
    }
    public List<EnemyView> GetEnemyViews() {
        return entityViews;
    }
}
