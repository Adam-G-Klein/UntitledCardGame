using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;

public class CombatEncounterView : GenericSingleton<CombatEncounterView>,
    IEntityViewDelegate
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

    private List<IUIEventReceiver> pickingModePositionList = new List<IUIEventReceiver>();
    [SerializeField]
    private float pulseAnimationSpeed = 0.1f;
    public bool animateHearts = false;

    private List<IEnumerator> pulseAnimations = new List<IEnumerator>();

    [SerializeField]
    private GameObject cardViewUIPrefab;

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
            Debug.Log("Nuking view and recreating");
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
            foreach (IUIEventReceiver view in pickingModePositionList) {
                view.SetPickingModes();
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
        return newEntityView.entityContainer;
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
}
