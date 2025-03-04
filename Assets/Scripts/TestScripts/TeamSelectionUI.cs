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
        // Randomly choose 3 of the common companions for the starting team.
        // Random selection without replacement.
        if (randomStarterCompanionGen) {
            System.Random rnd = new();
            List<CompanionTypeSO> commoners = new List<CompanionTypeSO>(gameState.baseShopData.companionPool.commonCompanions);
            CompanionListVariableSO chosen = new();
            chosen.activeCompanions = new List<Companion>();
            for (int i = 0; i < 3; i++) {
                int chosenIndex = rnd.Next(commoners.Count);
                CompanionTypeSO chosenCompanion = commoners[chosenIndex];
                commoners.Remove(chosenCompanion);

                chosen.activeCompanions.Add(new Companion(chosenCompanion));
            }
            team1ActiveCompanions = chosen;
        } else {
            team1ActiveCompanions = testTeamActiveCompanions;
        }
        docRenderer = GetComponent<UIDocumentScreenspace>();

        root = GetComponent<UIDocument>().rootVisualElement;
        updateState();

        var next = root.Q<UnityEngine.UIElements.Button>("Next");
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Position);
        next.clicked += () => initializeRun();
    }

    private void updateState() {
        makeTeamView(root.Q<VisualElement>("CompanionPortaitsContainer"), team1ActiveCompanions.GetCompanionTypes());
        makeInfoView(root.Q<VisualElement>("InfoContainer"), team1ActiveCompanions.GetCompanionTypes()[currentlySelectedCompanion]);
        docRenderer.SetStateDirty();
    }
    public void initializeRun()
    {
        gameState.companions.activeCompanions = new List<Companion>();
        gameState.companions.benchedCompanions = new List<Companion>();
        gameState.companions.currentCompanionSlots = 3;

        foreach (CompanionTypeSO companionType in team1ActiveCompanions.GetCompanionTypes())
        {
            gameState.companions.activeCompanions.Add(new Companion(companionType));
        }

        gameState.LoadNextLocation();
    }

    private void makeTeamView(VisualElement container, List<CompanionTypeSO> companionTypes)
    {
        foreach (CompanionTypeSO companionType in companionTypes)
        {
            container.Add(makeCharacterView(companionType));
        }
    }
    private VisualElement makeCharacterView(CompanionTypeSO companionType) {
        var container = new VisualElement();
        container.AddToClassList("companion-info-container");

        Debug.Log("Making companion view on team signing page");
        container.AddManipulator(new Clickable(evt => companionClicked(companionType)));
        if (currentlySelectedCompanion == team1ActiveCompanions.GetCompanionTypes().IndexOf(companionType))
        {
            container.AddToClassList("companion-info-container-selected");
        }

        container.RegisterCallback<PointerEnterEvent>(PointerEnter);
        container.RegisterCallback<PointerLeaveEvent>(PointerLeave);

        var name = new Label();
        name.text = companionType.companionName;
        name.AddToClassList("companion-name-label");
        container.Add(name);

        var portrait = new VisualElement();
        portrait.AddToClassList("companion-portrait");
        portrait.style.backgroundImage = new StyleBackground(companionType.sprite);
        container.Add(portrait);
        container.name = companionType.name;

        companionMap[container.name] = companionType;

        /*var archetype = new Label();
        archetype.AddToClassList("companion-title-label");
        archetype.text = companionType.keepsakeTitle;
        container.Add(archetype);*/

        contentToRedraw.Add(container);
        return container;
    }

    private void makeInfoView(VisualElement container, CompanionTypeSO companionType)
    {
        //VisualElement keepImg = container.Q<VisualElement>("KeepsakeImage");
        //keepImg.style.backgroundImage = new StyleBackground(companionType.keepsake);
        container.Q<Label>("keepsakeName").text = "Companion Ability";
        container.Q<Label>("keepsakeDesc").text = companionType.keepsakeDescription;

        VisualElement cards = container.Q<VisualElement>("cardsContainer");

        for (var i = 0; i < companionType.startingDeck.cards.Count; i++) {
            CardType card = companionType.startingDeck.cards[i];
            VisualElement cardView = new CardView(card, companionType).cardContainer;
            cardView.AddToClassList("team-signing-card-container");
            cardView.RegisterCallback<PointerEnterEvent>(PointerEnter);
            cardView.RegisterCallback<PointerLeaveEvent>(PointerLeave);
            cardView.name = card.name + i;
            cards.Add(cardView);
            contentToRedraw.Add(cardView);
            cardTypeMap[cardView.name] = card;
        }
    }

    private void PointerEnter(PointerEnterEvent evt) {
        VisualElement VE = evt.target as VisualElement;
        bool isCompanion = companionMap.ContainsKey(VE.name);
        if (!isCompanion && cardTypeMap[VE.name].tooltips.Count == 0) return;
        if (tooltipMap.ContainsKey(VE.name)) return;
        
        Vector3 tooltipPosition = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(VE);

        if (isCompanion) {
            tooltipPosition.x -= VE.resolvedStyle.width / 300; // this feels super brittle 
            tooltipPosition.y += VE.resolvedStyle.width / 400;
        } else {
            tooltipPosition.x -= VE.resolvedStyle.width / 150; // this feels super brittle 
            tooltipPosition.y += VE.resolvedStyle.width / 150;
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

    private VisualElement makeCardView(CardType card) {
        var container = new VisualElement();
        container.AddToClassList("team-signing-card-container");
        container.AddToClassList("card-container");

        var manaCost = new Label();
        manaCost.AddToClassList("mana-card-label");
        manaCost.text = card.Cost.ToString();
        container.Add(manaCost);

        var image = new VisualElement();
        image.AddToClassList("card-image");
        image.style.backgroundImage = new StyleBackground(card.Artwork);
        container.Add(image);

        var name = new Label();
        name.AddToClassList("card-title-label");
        name.text = card.Name;
        container.Add(name);

        var desc = new Label();
        desc.AddToClassList("card-desc-label");
        desc.text = card.Description;
        container.Add(desc);

        contentToRedraw.Add(container);
        return container;
    }

    public void companionClicked(CompanionTypeSO companionType) {
        Debug.Log("companion clicked on team selection screen");
        foreach (VisualElement content in contentToRedraw) {
            content.RemoveFromHierarchy();
        }
        currentlySelectedCompanion = team1ActiveCompanions.GetCompanionTypes().IndexOf(companionType);
        updateState();
     }

    public void backButtonHandler() {
        SceneManager.LoadScene("MainMenu");
    }
}
