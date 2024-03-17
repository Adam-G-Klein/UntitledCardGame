using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TeamSelection : MonoBehaviour
{
    public GameStateVariableSO gameState;
    private CompanionListVariableSO teamCompanions;
    private VisualElement root;
    [SerializeField]
    private bool displayOnStart = true;
    [SerializeField]
    private GameObject bookImageGO;
    private int maxTeamSlots;


    private void OnEnable()
    {
        teamCompanions = gameState.companions;
        maxTeamSlots = gameState.companions.currentCompanionSlots;
        root = GetComponent<UIDocument>().rootVisualElement;
        if(!displayOnStart) {
            root.style.display = DisplayStyle.None;
        }
        root.Q<UnityEngine.UIElements.Button>("close-button").clicked += () => toggleDisplay();
        makeActiveTeam(root.Q<VisualElement>("active-team-content"), teamCompanions.activeCompanions);
        makeBenchTeam(root.Q<VisualElement>("bench-content"), teamCompanions.benchedCompanions);
    }

    private void makeActiveTeam(VisualElement container, List<Companion> companions)
    {
        root.Q<Label>("ActiveTeamLabel").text = "Active Team (" + companions.Count + " / " + maxTeamSlots + ")";
        for(int i = 0; i < companions.Count; i++) {
            container.Add(makeActiveCharacterView(companions[i], i));
        }
    }

    private void makeBenchTeam(VisualElement container, List<Companion> companions)
    {
        for(int i = 0; i < companions.Count; i++) {
            container.Add(makeBenchCharacterView(companions[i]));
        }
    }
    private VisualElement makeActiveCharacterView(Companion companion, int index) {
        var container = new VisualElement();
        container.AddToClassList("active-team-container");

        var verticalButtonContainer = new VisualElement();
        verticalButtonContainer.AddToClassList("vertical-arrow-container");
        container.Add(verticalButtonContainer);

        if (index > 0) {
            var upArrow = new UnityEngine.UIElements.Button();
            upArrow.AddToClassList("arrow-up");
            upArrow.clicked += () => UpArrowPressed(companion, index);
            verticalButtonContainer.Add(upArrow);
        }
        if (index <=  maxTeamSlots && teamCompanions.activeCompanions.Count > 1) {
            var downArrow = new UnityEngine.UIElements.Button();
            downArrow.AddToClassList("arrow-down");
            downArrow.clicked += () => DownArrowPressed(companion, index);
            verticalButtonContainer.Add(downArrow);
        }

        var charContainer = new VisualElement();
        charContainer.style.alignItems = Align.Center;
        var portrait = new VisualElement();
        portrait.AddToClassList("team-character-portrait");
        portrait.style.backgroundImage = new StyleBackground(companion.companionType.sprite);
        charContainer.Add(portrait);
        var healthLabel = new Label();
        healthLabel.style.fontSize = 10;
        healthLabel.style.color = Color.red;
        healthLabel.style.marginTop = 3;
        healthLabel.style.paddingTop = 0;
        healthLabel.text = companion.combatStats.currentHealth + " / " + companion.combatStats.maxHealth;
        charContainer.Add(healthLabel);

        container.Add(charContainer);
        var rightArrow = new UnityEngine.UIElements.Button();
        rightArrow.AddToClassList("arrow-right");
        rightArrow.clicked += () => RightArrowPressed(companion);
        container.Add(rightArrow);
    
        return container;
    }

    private VisualElement makeBenchCharacterView(Companion companion) {
        var container = new VisualElement();
        container.AddToClassList("character-bench-item");

        if (teamCompanions.activeCompanions.Count < maxTeamSlots) {
            var leftArrow = new UnityEngine.UIElements.Button();
            leftArrow.AddToClassList("arrow-left");
            leftArrow.clicked += () => LeftArrowPressed(companion);
            container.Add(leftArrow);
        }

        var portrait = new VisualElement();
        portrait.AddToClassList("bench-character-portrait");
        portrait.style.backgroundImage = new StyleBackground(companion.companionType.sprite);
        container.Add(portrait);
    
        return container;
    }

    public void UpArrowPressed(Companion companion, int index) {
        if (index < 1) {
            return;
        }
        var temp = teamCompanions.activeCompanions[index - 1];
        teamCompanions.activeCompanions[index - 1] = companion;
        teamCompanions.activeCompanions[index] = temp;
        refreshActiveTeam();
        
    }

    public void DownArrowPressed(Companion companion, int index) {
        if (index > 3) {
            return;
        }
        var temp = teamCompanions.activeCompanions[index + 1];
        teamCompanions.activeCompanions[index + 1] = companion;
        teamCompanions.activeCompanions[index] = temp;
        refreshActiveTeam();
    }

    public void RightArrowPressed(Companion companion) {
        // if (teamCompanions.benchedCompanions.Count >= 5) {
        //     return;
        // } 
        teamCompanions.benchedCompanions.Add(companion);
        teamCompanions.activeCompanions.Remove(companion);
        refreshActiveTeam();
        refreshBench();
    }
    public void LeftArrowPressed(Companion companion) {
        if (teamCompanions.activeCompanions.Count >= maxTeamSlots) {
             return;
        } 
        teamCompanions.benchedCompanions.Remove(companion);
        teamCompanions.activeCompanions.Add(companion);
        refreshActiveTeam();
        refreshBench();
    }

    public void refreshActiveTeam() {
        var content = root.Q<VisualElement>("active-team-content");
        var childCount = content.childCount;
        for(int i = 0; i < childCount; i++) {
            content.RemoveAt(0);
        }
        makeActiveTeam(content, teamCompanions.activeCompanions);
    }

    public void refreshBench() {
        var content = root.Q<VisualElement>("bench-content");
        var childCount = content.childCount;
        for(int i = 0; i < childCount; i++) {
            content.RemoveAt(0);
        }
        makeBenchTeam(content, teamCompanions.benchedCompanions);

    }

    public void toggleDisplay() {
        Debug.Log("Toggling display");
        if(root.style.display == DisplayStyle.None) {
            bookImageGO.SetActive(true);
            root.style.display = DisplayStyle.Flex;
        } else {
            bookImageGO.SetActive(false);
            root.style.display = DisplayStyle.None;
        }
    }

}
