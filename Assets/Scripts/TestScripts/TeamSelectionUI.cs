using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TeamSelectionUI : MonoBehaviour
{
    public GameStateVariableSO gameState;
    // TODO: package these into companionListVariables
    public CompanionListVariableSO team1ActiveCompanions;
    public CompanionListVariableSO team1BenchCompanions;
    public CompanionListVariableSO team2ActiveCompanions;
    public CompanionListVariableSO team2BenchCompanions;
    public GameObject deckViewUIPrefab;
    private VisualElement root;
    [SerializeField]
    private bool displayOnStart = true;


    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        if(!displayOnStart) {
            root.style.display = DisplayStyle.None;
        }
        root.Q<Button>("backButton").clicked += backButtonHandler;
        makeTeamView(root.Q<VisualElement>("team-1-container"), team1ActiveCompanions.GetCompanionTypes());
        makeTeamView(root.Q<VisualElement>("team-2-container"), team2ActiveCompanions.GetCompanionTypes());
    }
    public void initializeRun(List<CompanionTypeSO> team)
    {
        gameState.companions.companionBench = new List<Companion>();
        gameState.companions.companionList = new List<Companion>();
        gameState.companions.currentCompanionSlots = 3;

        foreach (CompanionTypeSO companionType in team)
        {
            gameState.companions.companionList.Add(new Companion(companionType));
        }

        gameState.LoadNextLocation();
    }

    private void makeTeamView(VisualElement container, List<CompanionTypeSO> companionTypes)
    {
        foreach (CompanionTypeSO companionType in companionTypes)
        {
            container.Add(makeCharacterView(companionType));
        }

        //Redo this when more details
        //var centeringWrapper = new VisualElement();
        //centeringWrapper.style.alignItems = Align.Center;
        //container.Add(centeringWrapper);
        var confirm = new Button();
        confirm.text = "Sign this team";
        confirm.style.alignSelf = Align.Center;
        confirm.style.marginTop = 20;
        confirm.clicked += () => initializeRun(companionTypes);
        container.Add(confirm);
    }
    private VisualElement makeCharacterView(CompanionTypeSO companionType) {
        var container = new VisualElement();
        container.AddToClassList("character-container");

        var portrait = new VisualElement();
        portrait.AddToClassList("character-portrait");
        portrait.style.backgroundImage = new StyleBackground(companionType.sprite);
        container.Add(portrait);

        var textContainer = new VisualElement();
        textContainer.AddToClassList("text-container");
   
        var name = new Label();
        name.text = companionType.companionName;
        stripMarginAndPadding(name).AddToClassList("character-name");
        textContainer.Add(name);

        var archetype = new Label();
        stripMarginAndPadding(archetype).AddToClassList("archetype-name");
        archetype.text = "IDK probably something";
        textContainer.Add(archetype);

        var desc = new Label();
        stripMarginAndPadding(desc).AddToClassList("description");
        desc.text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. lmao gottem";
        textContainer.Add(desc);

        container.Add(textContainer);


        return container;
    }

    private Label stripMarginAndPadding(Label label) {
        label.style.marginBottom = 0;
        label.style.marginTop = 0;
        label.style.paddingBottom = 0;
        label.style.paddingTop = 0;
        return label;    
    }

    public void backButtonHandler() {
        SceneManager.LoadScene("MainMenu");
    }

    public void toggleDisplay() {
        Debug.Log("Toggling display");
        if(root.style.display == DisplayStyle.None) {
            root.style.display = DisplayStyle.Flex;
        } else {
            root.style.display = DisplayStyle.None;
        }
    }

}
