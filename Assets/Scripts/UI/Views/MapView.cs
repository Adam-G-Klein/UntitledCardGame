using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.VisualScripting;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

public class MapView
{
    public VisualElement mapContainer;
    // TODO, could require the stylesheet in the constructor and fetch these from there
    public Color activeColor = Color.green;
    private List<Encounter> encounterList = new();

    public MapView(IEncounterBuilder encounterBuilder)
    {
        if (encounterBuilder is EnemyEncounterManager) encounterList = EnemyEncounterManager.Instance.gameState.map.GetValue().encounters;
        if (encounterBuilder is ShopManager) encounterList = ShopManager.Instance.gameState.map.GetValue().encounters;
        mapContainer = makeMapView(encounterBuilder);
    }

    private bool IsEliteEncounter(Encounter encounter)
    {
        if (encounter.getEncounterType() == EncounterType.Enemy)
        {
            EnemyEncounter EE = (EnemyEncounter)encounter;
            return EE.isEliteEncounter;
        }
        return false;
    }
    private VisualElement makeMapView(IEncounterBuilder encounterBuilder)
    {
        // get current sceneIndex from gameState somehow;
        int curEncounterIndex = 0;
        if (encounterBuilder is EnemyEncounterManager) curEncounterIndex = EnemyEncounterManager.Instance.gameState.currentEncounterIndex;
        if (encounterBuilder is ShopManager) curEncounterIndex = ShopManager.Instance.gameState.currentEncounterIndex;
        VisualElement container = GameplayConstantsSingleton.Instance.gameplayConstants.mapTemplate.CloneTree();
        VisualElement mapSectionsRoot = container.Q("mapIcons");
        mapSectionsRoot.Clear();
        // loop until we get to the next elite encounter ... that's how far we want to display
        int startIndex, endIndex = 0;
        int temp = Math.Max(curEncounterIndex - 1, 0);
        while (temp > 0)
        {
            Encounter encounter = encounterList[temp];
            if (IsEliteEncounter(encounter))
            {
                temp++;
                break;
            }
            temp--;
        }
        startIndex = temp;
        temp = curEncounterIndex;
        while (temp < encounterList.Count() - 1)
        {
            Encounter encounter = encounterList[temp];
            if (IsEliteEncounter(encounter)) break;
            temp++;
        }
        endIndex = temp;

        for (int i = startIndex; i <= endIndex; i++)
        {
            Encounter encounter = encounterList[i];
            if (encounter.getEncounterType() != EncounterType.Enemy) continue;
            VisualElement mapSection = new VisualElement();
            mapSection.AddToClassList("map-section");
            VisualElement mapIcon = new VisualElement();
            mapIcon.AddToClassList("map-symbol");
            mapSection.Add(mapIcon);

            // if we've already complete the encounter than it should be light gray
            if (IsEliteEncounter(encounter))
            {
                mapIcon.style.backgroundImage = new StyleBackground(((EnemyEncounter)encounter).enemyList[0].enemyType.sprite);
                if (curEncounterIndex + 1 == i)
                {
                    mapIcon.AddToClassList("upcoming-elite-encounter");
                }
                else if (curEncounterIndex == i)
                {
                    mapIcon.AddToClassList("active-elite-encounter");
                }
            }
            else if (i < curEncounterIndex)
            {
                mapIcon.AddToClassList("map-icon-completed");
            }
            else if (curEncounterIndex == i)
            {
                mapIcon.AddToClassList("map-icon-active");
            }
            else if (i == curEncounterIndex + 1)
            {
                mapIcon.AddToClassList("map-icon-upcoming");
            }
            else
            {
                mapIcon.AddToClassList("map-icon-locked");
            }

            mapSectionsRoot.Add(mapSection);
        }
        PopulateActAndEncounterNumbers(container, curEncounterIndex);
        if (encounterBuilder is ShopManager) container.Q<Label>("turnNumberLabel").style.visibility = Visibility.Hidden;

        return container.Children().First();
    }

    private void PopulateActAndEncounterNumbers(VisualElement container, int curEncounterIndex)
    {
        int temp = curEncounterIndex - 1;
        int combatsFoughtThisFloor = 1;
        int elitesFought = 1;
        bool foundElite = false;
        while (temp >= 0)
        {
            Encounter encounter = encounterList[temp];
            if (IsEliteEncounter(encounter))
            {
                foundElite = true;
                elitesFought++;
            }
            if (encounter.getEncounterType() == EncounterType.Enemy && !foundElite)
            {
                combatsFoughtThisFloor++;
            }
            temp--;
        }
        Label actAndEncounterLabel = container.Q<Label>("encounterNumberLabel");
        actAndEncounterLabel.text = elitesFought.ToString() + "–" + combatsFoughtThisFloor.ToString();
    }

    public void UpdateTurnCounter(int number) {
        mapContainer.Q<Label>("turnNumberLabel").text = "Turn " +number.ToString();
    }
}
