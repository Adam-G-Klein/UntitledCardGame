using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GenerateMap : MonoBehaviour
{
    public MapGeneratorSO mapGenerator;
    public MapVariableSO activeMapVariable;
    public CompanionListVariableSO activeCompanionList;
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
    public void goWith(List<CompanionTypeSO> team)
    {
        activeCompanionList.companionBench = new List<Companion>();
        activeCompanionList.companionList = new List<Companion>();
        activeCompanionList.currentCompanionSlots = 3;

        playerData.GetValue().gold = 3;

        foreach (CompanionTypeSO companionType in team)
        {
            activeCompanionList.companionList.Add(new Companion(companionType));
        }

        activeMapVariable.SetValue(mapGenerator.generateMap());
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
        confirm.clicked += () => goWith(companionTypes);
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

    // Potentially obsolete
    public void go()
    {
        activeCompanionList.companionBench = new List<Companion>();
        activeCompanionList.companionList = new List<Companion>();
        activeCompanionList.currentCompanionSlots = 3;

        playerData.GetValue().gold = 3;

        foreach (CompanionTypeSO companionType in team1ActiveCompanions)
        {
            activeCompanionList.companionList.Add(new Companion(companionType));
        }

        foreach (CompanionTypeSO companionType in team1BenchCompanions)
        {
            activeCompanionList.companionBench.Add(new Companion(companionType));
        }

        activeMapVariable.SetValue(mapGenerator.generateMap());
        SceneManager.LoadScene("Map");
    }
}
