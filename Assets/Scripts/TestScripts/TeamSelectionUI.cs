using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;

public class TeamSelectionUI : MonoBehaviour
{
    public GameStateVariableSO gameState;
    // TODO: package these into companionListVariables
    public CompanionListVariableSO team1ActiveCompanions;
    public CompanionListVariableSO testTeamActiveCompanions;
    public GameObject deckViewUIPrefab;
    private VisualElement root;
    [SerializeField]
    private bool displayOnStart = true;
    [SerializeField]
    private GameObject tooltipPrefab;
    private int currentlySelectedCompanion = 0;
    private List<VisualElement> contentToRedraw = new List<VisualElement>();

    public bool randomStarterCompanionGen = true;

    public UIDocumentScreenspace docRenderer;
    private Dictionary<String, GameObject> tooltipMap = new();
    private Dictionary<String, CardType> cardTypeMap = new();
    private Dictionary<String, CompanionTypeSO> companionMap = new();

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
            for (int i = 0; i < 3; i++)
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
        updateState();
        makeTeamView(root.Q<VisualElement>("CompanionPortaitsContainer"), team1ActiveCompanions.GetCompanionTypes());

        var next = root.Q<UnityEngine.UIElements.Button>("Next");
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Position);
        next.RegisterOnSelected(() => initializeRun());
        FocusManager.Instance.RegisterFocusableTarget(next.AsFocusable());
    }

    private void updateState() {
        // destroy all tooltips
        foreach (var tooltip in tooltipMap.Values) {
            Destroy(tooltip);
        }
        makeInfoView(root.Q<VisualElement>("InfoContainer"), team1ActiveCompanions.GetCompanionTypes()[currentlySelectedCompanion]);
        docRenderer.SetStateDirty();
        root = GetComponent<UIDocument>().rootVisualElement;
    }

    public void initializeRun()
    {
        gameState.companions.activeCompanions = new List<Companion>();
        gameState.companions.benchedCompanions = new List<Companion>();
        gameState.companions.currentCompanionSlots = 3;

        foreach (CompanionTypeSO companionType in team1ActiveCompanions.GetCompanionTypes())
        {
            Companion companion = new Companion(companionType);
            if (ProgressManager.Instance.IsFeatureEnabled(AscensionType.DAMAGED_COMPANIONS)) {
                companion.combatStats.currentHealth -= (int)ProgressManager.Instance.GetAscensionSO(AscensionType.DAMAGED_COMPANIONS).ascensionModificationValues.GetValueOrDefault("healthReduction", 3f);
            }
            gameState.companions.activeCompanions.Add(companion);
        }

        gameState.LoadNextLocation();
    }

    private void makeTeamView(VisualElement container, List<CompanionTypeSO> companionTypes)
    {
        foreach (CompanionTypeSO companionType in companionTypes)
        {
            container.Add(makeCharacterView(companionType));
        }
        container.AddToClassList("companion-portraits-container");
    }

    private VisualElement makeCharacterView(CompanionTypeSO companionType) {
        var container = new VisualElement();
        container.AddToClassList("companion-info-container");
        container.MakeFocusable();

        Debug.Log("Making companion view on team signing page");
        container.AddManipulator(new Clickable(evt => companionClicked(companionType)));
        if (currentlySelectedCompanion == team1ActiveCompanions.GetCompanionTypes().IndexOf(companionType))
        {
            container.AddToClassList("companion-info-container-selected");
        }

        container.RegisterCallback<PointerEnterEvent>(PointerEnter);
        container.RegisterCallback<PointerLeaveEvent>(PointerLeave);
        container.RegisterOnSelected(() => companionClicked(companionType));

        VisualElementFocusable containerFocusable = container.AsFocusable();
        containerFocusable.additionalFocusAction += () => PointerEnter(container.CreateFakePointerEnterEvent());
        containerFocusable.additionalUnfocusAction += () => PointerLeave(container.CreateFakePointerLeaveEvent());
        FocusManager.Instance.RegisterFocusableTarget(containerFocusable);

        var name = new Label();
        name.text = companionType.companionName;
        name.AddToClassList("companion-name-label");
        container.Add(name);

        var portrait = new VisualElement();
        portrait.AddToClassList("companion-portrait");
        portrait.style.backgroundImage = new StyleBackground(companionType.fullSprite);
        container.Add(portrait);
        container.name = companionType.name;

        companionMap[container.name] = companionType;

        //contentToRedraw.Add(container);
        return container;
    }

    private void makeInfoView(VisualElement container, CompanionTypeSO companionType)
    {
        container.Q<Label>("keepsakeName").text = "Companion Ability";
        container.Q<Label>("keepsakeDesc").text = companionType.keepsakeDescription;

        VisualElement cards = container.Q<VisualElement>("cardsContainer");

        for (var i = 0; i < companionType.startingDeck.cards.Count; i++) {
            CardType card = companionType.startingDeck.cards[i];
            VisualElement cardView = new CardView(card, companionType, Card.CardRarity.COMMON, true).cardContainer;
            cardView.AddToClassList("team-signing-card-container");
            cardView.RegisterCallback<PointerEnterEvent>(PointerEnter);
            cardView.RegisterCallback<PointerLeaveEvent>(PointerLeave);

            cardView.MakeFocusable();
            VisualElementFocusable cardViewFocusable = cardView.AsFocusable();
            cardViewFocusable.additionalFocusAction += () => PointerEnter(cardView.CreateFakePointerEnterEvent());
            cardViewFocusable.additionalUnfocusAction += () => PointerLeave(cardView.CreateFakePointerLeaveEvent());
            FocusManager.Instance.RegisterFocusableTarget(cardViewFocusable);

            cardView.name = card.name + i;
            cards.Add(cardView);
            contentToRedraw.Add(cardView);
            cardTypeMap[cardView.name] = card;
        }
    }

    private void PointerEnter(PointerEnterEvent evt) {
        VisualElement VE = evt.target as VisualElement;
        bool isCompanion = companionMap.ContainsKey(VE.name);
        if (isCompanion)
        {
            int hoveredIndex = team1ActiveCompanions.GetCompanionTypes().IndexOf(companionMap[VE.name]);
            if (currentlySelectedCompanion != hoveredIndex)
            {
                companionClicked(companionMap[VE.name]);
            }

        }
        if (!isCompanion && cardTypeMap[VE.name].GetTooltip().empty) return;
        if (tooltipMap.ContainsKey(VE.name)) return;

        Vector3 tooltipPosition;

        if (isCompanion) {
            float xTooltipPos = VE.worldBound.center.x - (VE.resolvedStyle.width * .85f);
            float yTooltipPos = VE.worldBound.center.y + (VE.resolvedStyle.height * .2f);
            Vector3 position = new Vector3(xTooltipPos, yTooltipPos, 0);

            tooltipPosition = UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(position);
        } else {
            float xTooltipPos = VE.worldBound.center.x - (VE.resolvedStyle.width * 1.1f);
            float yTooltipPos = VE.worldBound.center.y + (VE.resolvedStyle.height * .1f);
            Vector3 position = new Vector3(xTooltipPos, yTooltipPos, 0);

            tooltipPosition = UIDocumentGameObjectPlacer.GetWorldPositionFromUIDocumentPosition(position);
        }
        tooltipPosition.z = -2; // THIS SHOULD NOT BE NECESSARY BUT NO OTHER LAYERING WAS WORKING

        GameObject uiDocToolTipPrefab = Instantiate(tooltipPrefab, tooltipPosition, new Quaternion());
        TooltipView tooltipView = uiDocToolTipPrefab.GetComponent<TooltipView>();

        tooltipView.tooltip = isCompanion ? companionMap[VE.name].tooltip : cardTypeMap[VE.name].GetTooltip();

        tooltipMap.Add(VE.name, uiDocToolTipPrefab);
    }

    private void PointerLeave(PointerLeaveEvent evt) {
        VisualElement VE = evt.target as VisualElement;
        if (tooltipMap.ContainsKey(VE.name)) {
            Destroy(tooltipMap[VE.name]);
            tooltipMap.Remove(VE.name);
        }
    }

    public void companionClicked(CompanionTypeSO companionType) {
        foreach (VisualElement content in contentToRedraw) {
            content.RemoveFromHierarchy();
            FocusManager.Instance.UnregisterFocusableTarget(content.AsFocusable());
        }
        currentlySelectedCompanion = team1ActiveCompanions.GetCompanionTypes().IndexOf(companionType);
        updateState();
     }

    public void backButtonHandler() {
        SceneManager.LoadScene("MainMenu");
    }
}
