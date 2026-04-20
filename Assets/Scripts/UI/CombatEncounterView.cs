using System;
using System.Collections;
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
    public List<EnemyView> EnemyViews => entityViews;
    private List<CompanionView> companionViews = new List<CompanionView>();
    public List<CompanionView> CompanionViews => companionViews;


    [SerializeField]
    private GameObject cardViewUIPrefab;
    [SerializeField]
    private GameObject newCardViewUIPrefab;
    [SerializeField]
    private GameObject victoryPopupPrefab;
    [SerializeField]
    private GameObject defeatPopupPrefab;
    private bool inMenu = false;
    private bool inDeckView = false;
    private bool combatOver = false;
    public MapView mapView;
    private UnityEngine.UIElements.Button endTurnButton;

    private bool hideAllElements = false;
    private Dictionary<CombatInstance, CompanionView> combatInstanceToCompanionView;
    private Dictionary<CombatInstance, EnemyView> combatInstanceToEnemyView;
    private bool bossFight = false;

    public void SetupFromGamestate(EnemyEncounterManager enemyEncounterManager, bool hideAllElements = false)
    {
        this.hideAllElements = hideAllElements;
        InitNonEntityElements(enemyEncounterManager);
        InitCompanions();
        InitEnemies();
        setupComplete = true;
    }

    public void SetupTutorial(EnemyEncounterManager enemyEncounterManager) {
        this.enemyEncounterManager = enemyEncounterManager;
    }

    public void InitNonEntityElements(EnemyEncounterManager enemyEncounterManager) {
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
        
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Ignore);

        endTurnButton = root.Q<UnityEngine.UIElements.Button>("end-turn");
        endTurnButton.pickingMode = PickingMode.Position;
        endTurnButton.RegisterOnSelected(EndPlayerTurnHandler);
        IconButton endTurnIcon = endTurnButton as IconButton;
        endTurnIcon.SetIcon(GFGInputAction.END_TURN, ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.END_TURN));
        ControlsManager.Instance.RegisterIconChanger(endTurnIcon);

        // for the boss fight
        VisualElement mapRoot = root.Q("mapRoot");
        mapRoot.Clear();
        mapView = new MapView(gameState.map.GetValue(), gameState.currentEncounterIndex, EncounterType.Enemy);
        mapView.UpdateMoneyAmount(gameState.playerData.GetValue().gold);
        mapRoot.Add(mapView.mapContainer);

        IconButton deckViewButton = root.Q<IconButton>("deck-view-button");
        deckViewButton.RegisterOnSelected(ShowDeckView);
        FocusManager.Instance.RegisterFocusableTarget(deckViewButton.AsFocusable());
        deckViewButton.SetIcon(
            GFGInputAction.OPEN_MULTI_DECK_VIEW,
            ControlsManager.Instance.GetSpriteForGFGAction(GFGInputAction.OPEN_MULTI_DECK_VIEW));
        ControlsManager.Instance.RegisterIconChanger(deckViewButton);
        deckViewButton.pickingMode = PickingMode.Position;
    }

    public void InitCompanions() {
        List<Companion> companions = gameState.companions.activeCompanions;
        SetupCompanions(root.Q<VisualElement>("companionContainer"), companions);
    }

    public void InitEnemies() {
        List<Enemy> enemies = ((EnemyEncounter)gameState.activeEncounter.GetValue()).enemyList;
        setupEnemies(root.Q<VisualElement>("enemyContainer"), enemies);
    }

    public void ShowOnlyCompanionSprites() {
        foreach (CompanionView comp in companionViews) {
            comp.SetOnlySpriteVisible();
        }
    }

    public void SetPersistentElementsVisible(bool visible)
    {
        if(root == null) {
            uiDoc = GetComponent<UIDocument>();
            root = uiDoc.rootVisualElement;
        }
        DisplayStyle displayStyle = visible ? DisplayStyle.Flex : DisplayStyle.None;

        VisualElement mapRoot = root.Q<VisualElement>("mapRoot");
        if (mapRoot == null) Debug.LogWarning("SetPersistentElementsVisible: could not find 'mapRoot'");
        else mapRoot.parent.style.display = displayStyle;

        VisualElement endTurn = root.Q<VisualElement>("end-turn");
        if (endTurn == null) Debug.LogWarning("SetPersistentElementsVisible: could not find 'end-turn'");
        else endTurn.style.display = displayStyle;

        VisualElement mamna = root.Q<VisualElement>("mamna");
        if (mamna == null) Debug.LogWarning("SetPersistentElementsVisible: could not find 'mamna'");
        else mamna.style.display = displayStyle;

        VisualElement deckViewButton = root.Q<VisualElement>("deck-view-button");
        if (deckViewButton == null) Debug.LogWarning("SetPersistentElementsVisible: could not find 'deck-view-button'");
        else deckViewButton.style.display = displayStyle;
    }

    public void SetManaIndicatorVisible() {
        VisualElement mamna = root.Q<VisualElement>("mamna");
        if (mamna == null) Debug.LogWarning("SetPersistentElementsVisible: could not find 'mamna'");
        else mamna.style.display = DisplayStyle.Flex;
    }

    // for boss fight intro
    public void DestroyMapAndEnemyUI()
    {
        VisualElement mapRoot = root.Q("mapRoot");
        // display none
        mapRoot.style.display = DisplayStyle.None;
        VisualElement enemyContainer = root.Q<VisualElement>("enemyContainer");
        FocusManager.Instance.UnregisterFocusables(enemyContainer);
        enemyContainer.style.display = DisplayStyle.None;
    }

    // for boss fight outro
    public void RecreateMapAndEnemyUI()
    {
        VisualElement mapRoot = root.Q("mapRoot");
        mapRoot.style.display = DisplayStyle.Flex;
        VisualElement enemyContainer = root.Q<VisualElement>("enemyContainer");
        // don't need focusables back, combat is over
        //FocusManager.Instance.RegisterFocusables(enemyContainer);
        enemyContainer.style.display = DisplayStyle.Flex;
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

    public void DisableInteractions() {
        foreach (EnemyView entityView in entityViews) {
            entityView.DisableInteractions();
        }
        foreach (CompanionView view in companionViews)
        {
            view.DisableInteractions();
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

    public void UpdateSpriteForEnemy(Enemy enemy, Sprite sprite)
    {
        EnemyView enemyView = entityViews.Find(ev => ev.GetEnemy() == enemy);
        if (enemyView != null)
        {
            Debug.Log($"Updating sprite for enemy {enemy.GetName()}");
            enemyView.UpdateSprite(sprite);
        }
    }

    // This function runs the first frame, which creates the enemy views before the enemy instances
    // exist.
    private void setupEnemies(VisualElement container, IEnumerable<Enemy> enemies)
    {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (var entity in enemies) {
            EnemyView enemyView = setupEnemy(entity, index);
            container.Add(enemyView.container);
            // for when we have more bosses or different enemy types that need special handling
            switch (entity.GetDisplayType())
            {
                case DisplayType.MEOTHRA:
                    bossFight = true;
                    break;
                default:
                    break;
            }

            if (hideAllElements) {
                enemyView.SetEverythingHidden();
                enemyView.DisableInteractions();
            }

            index++;
        }
    }

    private void SetupEnemies(VisualElement container, IEnumerable<EnemyInstance> enemyInstances)
    {
        var index = UIDocumentGameObjectPlacer.INITIAL_INDEX;
        foreach (EnemyInstance enemyInstance in enemyInstances)
        {
            // setupEnemy adds to the list
            EnemyView enemyView = setupEnemy(enemyInstance.enemy, index, enemyInstance);
            enemyInstance.enemyView = enemyView;
            container.Add(enemyView.container);
            combatInstanceToEnemyView.Add(enemyInstance.combatInstance, enemyView);

            if (hideAllElements) {
                enemyView.SetEverythingHidden();
                enemyView.DisableInteractions();
            }

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
            CompanionView companionView = SetupCompanion(companion, index);
            container.Insert(0, companionView.container);

            if (hideAllElements) {
                companionView.SetEverythingHidden();
                companionView.DisableInteractions();
            }

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

            if (hideAllElements) {
                companionView.SetEverythingHidden();
                companionView.DisableInteractions();
            }

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

    public void HighlightRelevantCompanions(CombatInstance combatInstance)
    {
        foreach (CompanionView companionView in companionViews)
        {
            bool turnOffGlow = combatInstance == null || combatInstanceToCompanionView[combatInstance] != companionView;
            if (!companionView.IsDead() && companionView.GetCompanionInstance() != null) {
                companionView.GetCompanionInstance().ToggleHighlightGlow(!turnOffGlow);
            }
        }
    }

    private EnemyView setupEnemy(Enemy enemy, int index, EnemyInstance enemyInstance = null)
    {
        EnemyView newEntityView = new EnemyView(enemy, index, this, enemyInstance);
        pickingModePositionList.Add(newEntityView);
        entityViews.Add(newEntityView);

        VisualElementFocusable entityViewFocusable = newEntityView.container.GetUserData<VisualElementFocusable>();
        entityViewFocusable.SetTargetType(Targetable.TargetType.Enemy);
        if (enemy.enemyType.enemyDisplayType == DisplayType.UIDOC) FocusManager.Instance.RegisterFocusableTarget(entityViewFocusable);

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

    public void SetEndCombat(bool isCombatOver = true)
    {
        combatOver = isCombatOver;
    }

    private void EndPlayerTurnHandler()
    {
        enemyEncounterManager.TryEndPlayerTurn();
    }

    public void ShowDeckView(ClickEvent evt)
    {
        if (combatOver) return;
        MultiDeckViewManager.Instance.ShowCombatDeckView();
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
            if (instance == null) continue;
            try
            {
                instance.GetComponent<TooltipOnHover>()?.OnPointerExitVoid();
            }
            catch (MissingReferenceException exception)
            {
                // If an enemy has died, we'll hit this case, but we don't want to fail outright
                Debug.LogWarning(exception);
            }
        }

        foreach (CombatInstance instance in combatInstanceToEnemyView.Keys)
        {
            if (instance == null) continue;
            try
            {
                instance.GetComponent<TooltipOnHover>()?.OnPointerExitVoid();
            }
            catch (MissingReferenceException exception)
            {
                // If an enemy has died, we'll hit this case, but we don't want to fail outright
                Debug.LogWarning(exception);
            }
        }
    }

    public List<EnemyView> GetEnemyViews() {
        return entityViews;
    }

    public IEnumerator DisplayVictory() {
        if (EnemyEncounterManager.Instance.isBoss == true) yield break;
        yield return new WaitForSeconds(0.25f);
        GameObject victory = Instantiate(victoryPopupPrefab, this.transform.parent);
        victory.transform.localPosition = new Vector3(0, 0, -100f);
        yield return new WaitForSeconds(3f);
        Destroy(victory);
    }

    public IEnumerator DisplayDefeat() {
        yield return new WaitForSeconds(0.25f);
        GameObject defeat = Instantiate(defeatPopupPrefab, this.transform.parent);
        defeat.transform.localPosition = new Vector3(0, 0, -100f);
        yield return new WaitForSeconds(3f);
        Destroy(defeat);
    }

    // For the onboarding experience
    public void ShowCardsFromCompanion(CompanionInstance companionInstance) {
        float duration = 0.75f; // 0.75 seconds
        long delay = 100;
        CompanionView companionView = combatInstanceToCompanionView[companionInstance.combatInstance];
        if (companionView == null) return;

        VisualElement cardArea = root.Q<VisualElement>("demo-card-view-area");
        cardArea.style.display = DisplayStyle.Flex;
        cardArea.style.visibility = Visibility.Visible;

        foreach (Card card in companionInstance.deckInstance.drawPile) {
            CardView cardView = new CardView(card, companionInstance.companion.companionType);
            cardArea.Add(cardView.cardContainer);
            cardView.cardContainer.style.visibility = Visibility.Hidden;
            cardView.cardContainer.schedule.Execute(() => {
                Vector2 delta = companionView.container.worldBound.center - cardView.cardContainer.worldBound.center;
                cardView.cardContainer.style.translate = new Translate(delta.x, delta.y);
                
                LeanTween.value(0f, 1f, duration)
                    .setOnUpdate((float val) => {
                        cardView.cardContainer.style.visibility = Visibility.Visible;
                        float xVal = Mathf.Lerp(delta.x, 0, val);
                        float yVal = Mathf.Lerp(delta.y, 0, val);
                        cardView.cardContainer.style.translate = new Translate(xVal, yVal);
                        cardView.cardContainer.style.scale = new Vector2(val, val);
                    });
            }).ExecuteLater(delay);
            delay += 100; // 400 ms
        }
    }

    public void HideCardsAndShowCompanionFrame(CompanionInstance companionInstance, Action callback) {
        float duration = 0.75f; // 0.75 seconds
        long delay = 100;
        CompanionView companionView = combatInstanceToCompanionView[companionInstance.combatInstance];
        if (companionView == null) return;

        companionView.FadeInFrame(duration);
        companionInstance.FadeInBackgroundGradient(duration);

        VisualElement cardArea = root.Q<VisualElement>("demo-card-view-area");
        List<IVisualElementScheduledItem> scheduledItems = new List<IVisualElementScheduledItem>();
        foreach (VisualElement el in cardArea.Children()) {
            IVisualElementScheduledItem scheduledItem = el.schedule.Execute(() => {
                Vector2 delta = companionView.container.worldBound.center - el.worldBound.center;
                LeanTween.value(0f, 1f, duration)
                    .setOnUpdate((float val) => {
                        float xVal = Mathf.Lerp(0, delta.x, val);
                        float yVal = Mathf.Lerp(0, delta.y, val);
                        el.style.translate = new Translate(xVal, yVal);
                        el.style.scale = new Vector2(1f-val, 1f-val);
                    })
                    .setOnComplete(() => {
                        el.style.visibility = Visibility.Hidden;
                        scheduledItems.RemoveAt(0);
                        if (scheduledItems.Count == 0) {
                            cardArea.Clear();
                            callback();
                        }
                    });
            });
            scheduledItems.Add(scheduledItem);
            scheduledItem.ExecuteLater(delay);
            delay += 100; // 400 ms
        }
    }

    public void ShowEnemyFrame(EnemyInstance enemy) {
        float duration = 0.75f; // 0.75 seconds
        EnemyView enemyView = combatInstanceToEnemyView[enemy.combatInstance];
        if (enemyView == null) return;

        enemyView.FadeInFrame(duration);
        enemy.FadeInBackgroundGradient(duration);
    }
}
