using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TeamSelectionUI : MonoBehaviour, ICompanionViewDelegate
{
    public GameStateVariableSO gameState;
    public CompanionListVariableSO team1ActiveCompanions;
    public CompanionListVariableSO testTeamActiveCompanions;
    public GameObject deckViewUIPrefab;
    private VisualElement root;
    private Button next;
    [SerializeField]
    private bool displayOnStart = true;
    [SerializeField]
    private GameObject tooltipPrefab;
    private TooltipController tooltipController;

    public bool randomStarterCompanionGen = true;

    public UIDocumentScreenspace docRenderer;

    private List<CompanionView> companionViews = new List<CompanionView>();

    private void Start()
    {
        // Let's do it this way: we will choose the packs at random sequentially without replacement,
        // then choose a companion randomly from the common companions in the pack.
        // Then, if there are no packs left, we will reset the list of packs.
        // Ideally there are enough common companions spread across the packs that this doesn't happen.
        if (randomStarterCompanionGen)
        {
            System.Random rnd = new();
            List<PackSO> packSOWithCommonCompanions = gameState.baseShopData.activePacks.Where(pack => pack.companionPoolSO.commonCompanions.Count > 0).ToList();
            List<PackSO> mutablePackSOs = new List<PackSO>(packSOWithCommonCompanions);
            CompanionListVariableSO chosen = new();
            chosen.activeCompanions = new List<Companion>();
            for (int i = 0; i < gameState.baseShopData.shopLevels[0].teamSize; i++)
            {
                int chosenIndex = rnd.Next(mutablePackSOs.Count);
                PackSO chosenPack = mutablePackSOs[chosenIndex];
                Debug.Log("StartingTeamPreview: Chose pack " + chosenPack.name + " for index " + i);
                mutablePackSOs.Remove(chosenPack);

                // Reset to the original list of packs if we run out of packs to pick from.
                if (mutablePackSOs.Count == 0)
                {
                    Debug.LogWarning("We ran out of packs to pick from for the starting team so we are resetting");
                    mutablePackSOs = new List<PackSO>(packSOWithCommonCompanions);
                }

                int chosenCompanionIdx = rnd.Next(chosenPack.companionPoolSO.commonCompanions.Count);
                CompanionTypeSO chosenCompanion = chosenPack.companionPoolSO.commonCompanions[chosenCompanionIdx];
                Debug.Log("StartingTeamPreview: Chose companion " + chosenCompanion.name + " for index " + i);

                chosen.activeCompanions.Add(new Companion(chosenCompanion));
            }
            team1ActiveCompanions = chosen;
        }
        else
        {
            team1ActiveCompanions = testTeamActiveCompanions;
        }
        docRenderer = GetComponent<UIDocumentScreenspace>();

        root = GetComponent<UIDocument>().rootVisualElement;
        InitializeCompanions(team1ActiveCompanions.activeCompanions);

        next = root.Q<Button>("Next");
        next.RegisterOnSelected(() => initializeRun());
        FocusManager.Instance.RegisterFocusableTarget(next.AsFocusable());

        tooltipController = new TooltipController(tooltipPrefab);

        if (gameState.buildType == BuildType.DEMO && !DemoDirector.Instance.IsStepCompleted(DemoStepName.BendingTheRules)) {
            DisableUI();
            StartCoroutine(DisplayDemoDialogue());
        }
    }

    private void DisableUI() {
        next.SetEnabled(false);
        FocusManager.Instance.DisableAll();
        foreach (CompanionView companion in companionViews) {
            companion.DisableInteractions();
        }
    }

    private void EnableUI() {
        next.SetEnabled(true);
        FocusManager.Instance.EnableAll();
        foreach (CompanionView companion in companionViews) {
            companion.EnableInteractions();
        }
    }

    private IEnumerator DisplayDemoDialogue() {
        yield return DemoDirector.Instance.InvokeDemoStepCorouutine(DemoStepName.BendingTheRules);
        EnableUI();
    }

    private void InitializeCompanions(List<Companion> companions) {
        VisualElement companionContainer =  root.Q<VisualElement>("CompanionPortaitsContainer");
        foreach (Companion companion in companions) {
            CompanionView companionView = new CompanionView(
                    companion,
                    EncounterConstantsSingleton.Instance.encounterConstantsSO.companionViewTemplate,
                    0,
                    CompanionView.STARTING_TEAM_CONTEXT,
                    this);
            companionView.ScaleView(1.5f);
            companionContainer.Add(companionView.container);
            companionView.container.RegisterOnFocused(() => tooltipController.DisplayTooltip(companionView.container, companion.companionType.GetTooltip(), TooltipContext.StartingTeam));
            companionView.container.RegisterOnUnfocused(() => tooltipController.DestroyTooltip(companionView.container));
            FocusManager.Instance.RegisterFocusableTarget(companionView.focusable);
            companionViews.Add(companionView);
        }
    }

    public void initializeRun()
    {
        gameState.companions.activeCompanions = new List<Companion>();
        gameState.companions.benchedCompanions = new List<Companion>();
        gameState.companions.currentCompanionSlots = gameState.baseShopData.shopLevels[0].teamSize;

        foreach (CompanionTypeSO companionType in team1ActiveCompanions.GetCompanionTypes())
        {
            Companion companion = new Companion(companionType);
            gameState.companions.activeCompanions.Add(companion);
        }

        gameState.LoadNextLocation();
    }

    // Unused unless in combat
    public Sprite GetStatusEffectSprite(StatusEffectType statusEffectType)
    {
        return null;
    }

    public void ViewDeck(DeckViewType deckViewType, Companion companion = null, CompanionInstance companionInstance = null)
    {
        DeckViewTab deckViewTab = new DeckViewTab
        {
            title = "Deck"
        };
        List<DeckViewTabSection> sections = new List<DeckViewTabSection>();
        int startingIndex = 0;

        for (int i = 0; i < team1ActiveCompanions.activeCompanions.Count; i++) {
            DeckViewTabSection section = new DeckViewTabSection
            {
                companion = team1ActiveCompanions.activeCompanions[i],
                cards = team1ActiveCompanions.activeCompanions[i].deck.cards
            };
            sections.Add(section);
            if (companion == team1ActiveCompanions.activeCompanions[i]) {
                startingIndex = i;
            }
        }
        deckViewTab.sections = sections;

        MultiDeckViewManager.Instance.OnViewEnter();
        MultiDeckViewManager.Instance.ShowView(new List<DeckViewTab>() { deckViewTab }, 0, startingIndex);
    }
}
