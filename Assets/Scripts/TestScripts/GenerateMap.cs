using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GenerateMap : MonoBehaviour
{
    public GameStateVariableSO gameState;
    public MapGeneratorSO mapGenerator;
    public PlayerDataVariableSO playerData;
    public List<CompanionTypeSO> team1ActiveCompanions;
    public List<CompanionTypeSO> team1BenchCompanions;
    public List<CompanionTypeSO> team2ActiveCompanions;
    public List<CompanionTypeSO> team2BenchCompanions;
    public GameObject deckViewUIPrefab;
    private VisualElement root;


    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("backButton").clicked += backButtonHandler;
        makeTeamView(root.Q<VisualElement>("team-1-container"), team1ActiveCompanions);
        makeTeamView(root.Q<VisualElement>("team-2-container"), team2ActiveCompanions);
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
        Debug.Log("got here!");

        // companionList isn't a VariableSO, so we have to set it manually
        gameState.map.SetValue(mapGenerator.generateMap());
        gameState.playerData.initializeRun();
        Debug.Log("got here2!");
        SceneManager.LoadScene("Map");
    }

    private void makeTeamView(VisualElement container, List<CompanionTypeSO> companionTypes)
    {
        foreach (CompanionTypeSO companionType in companionTypes)
        {
            container.Add(makeCharacterView(companionType));
        }

        //Redo this when more details
        var confirm = new Button();
        confirm.text = "Sign this team";
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
        name.AddToClassList("character-name");
        textContainer.Add(name);

        var archetype = new Label();
        archetype.AddToClassList("archetype-name");
        archetype.text = "IDK probably something";
        textContainer.Add(archetype);

        var desc = new Label();
        desc.AddToClassList("description");
        desc.text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. lmao gottem";
        textContainer.Add(desc);

        container.Add(textContainer);


        return container;
    }

    public void backButtonHandler() {
        SceneManager.LoadScene("MainMenu");
    }

}
