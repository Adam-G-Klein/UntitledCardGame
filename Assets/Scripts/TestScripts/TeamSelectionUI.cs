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
    public CompanionListVariableSO team1BenchCompanions;
    public GameObject deckViewUIPrefab;
    private VisualElement root;
    [SerializeField]
    private bool displayOnStart = true;
    private int currentlySelectedCompanion = 0;
    private List<VisualElement> contentToRedraw = new List<VisualElement>();

    public bool randomStarterCompanionGen = true;

    public UIDocumentScreenspace docRenderer;

    private void OnEnable()
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
        }
        docRenderer = GetComponent<UIDocumentScreenspace>();

        root = GetComponent<UIDocument>().rootVisualElement;
        UIDocumentUtils.SetAllPickingMode(root, PickingMode.Position);
        //root.Q<UnityEngine.UIElements.Button>("backButton").clicked += backButtonHandler;
        docRenderer.constantStateUpdate = true;
        /*
        EventCallback<MouseEnterEvent> mouseEnter = new EventCallback<MouseEnterEvent>(
            evt => {
                docRenderer.SetStateDirty();
            });

        EventCallback<MouseLeaveEvent> mouseLeave = new EventCallback<MouseLeaveEvent>(
            evt => {
                docRenderer.SetStateDirty();
            });
        UIDocumentUtils.SetAllMouseMotionCallbacks(root, mouseEnter, mouseLeave);
        */
        updateState();

        var next = root.Q<UnityEngine.UIElements.Button>("Next");
        next.clicked += () => initializeRun();
    }

    private void updateState() {
        makeTeamView(root.Q<VisualElement>("CompanionPortaitsContainer"), team1ActiveCompanions.GetCompanionTypes());
        makeInfoView(root.Q<VisualElement>("InfoContainer"), team1ActiveCompanions.GetCompanionTypes()[currentlySelectedCompanion]);
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

        container.AddManipulator(new Clickable(evt => companionClicked(companionType)));
        if (currentlySelectedCompanion == team1ActiveCompanions.GetCompanionTypes().IndexOf(companionType))
        {
            container.AddToClassList("companion-info-container-selected");
        }


        var portrait = new VisualElement();
        portrait.AddToClassList("companion-portrait");
        portrait.style.backgroundImage = new StyleBackground(companionType.sprite);
        container.Add(portrait);

        var name = new Label();
        name.text = companionType.companionName;
        name.AddToClassList("companion-name-label");
        container.Add(name);

        var archetype = new Label();
        archetype.AddToClassList("companion-title-label");
        archetype.text = companionType.keepsakeTitle;
        container.Add(archetype);

        contentToRedraw.Add(container);
        return container;
    }

    private void makeInfoView(VisualElement container, CompanionTypeSO companionType)
    {
        VisualElement keepImg = container.Q<VisualElement>("KeepsakeImage");
        keepImg.style.backgroundImage = new StyleBackground(companionType.keepsake);
        container.Q<Label>("keepsakeName").text = companionType.keepsakeTitle;
        container.Q<Label>("keepsakeDesc").text = companionType.keepsakeDescription;

        VisualElement cards = container.Q<VisualElement>("cardsContainer");

        foreach (CardType card in companionType.startingDeck.cards) {
          cards.Add(makeCardView(card));
        }
    }

    private VisualElement makeCardView(CardType card) {
        var container = new VisualElement();
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
