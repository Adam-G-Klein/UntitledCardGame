using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

public class MapView
{
    public VisualElement mapContainer;

    private VisualElement mapIconContainer;
    private Label actEncounterNumberLabel;
    private Label moneyLabel;
    private Label mainTextLabel;
    private VisualElement arrow;

    private Map map;
    private int currentEncounterIndex;
    private EncounterType encounterType;
    private VisualElement activeEncounterIcon;

    public MapView(Map map, int currentEncounterIndex, EncounterType encounterType)
    {
        this.map = map;
        this.currentEncounterIndex = currentEncounterIndex;
        this.encounterType = encounterType;
        mapContainer = MakeMapView();

        mapContainer.RegisterCallback<GeometryChangedEvent>(UpdateIconPositions);
    }
    
    private VisualElement MakeMapView()
    {
        VisualElement container = GameplayConstantsSingleton.Instance.gameplayConstants.mapTemplate.CloneTree();
        mapIconContainer = container.Q<VisualElement>("map-view-icon-area");
        actEncounterNumberLabel = container.Q<Label>("map-view-act-enc-label");
        moneyLabel = container.Q<Label>("map-view-money-label");
        mainTextLabel = container.Q<Label>("map-view-main-text-label");
        arrow = container.Q<VisualElement>("map-view-arrow");

        // loop until we get to the next elite encounter ... that's how far we want to display
        int startIndex, endIndex;
        int temp = Math.Max(currentEncounterIndex - 1, 0);
        while (temp > 0)
        {
            Encounter encounter = map.encounters[temp];
            if (IsEliteEncounter(encounter))
            {
                temp++;
                break;
            }
            temp--;
        }
        startIndex = temp;
        temp = currentEncounterIndex;
        while (temp < map.encounters.Count() - 1)
        {
            Encounter encounter = map.encounters[temp];
            if (IsEliteEncounter(encounter)) break;
            temp++;
        }
        endIndex = temp;

        for (int i = startIndex; i <= endIndex; i++)
        {
            Encounter encounter = map.encounters[i];
            if (encounter.getEncounterType() != EncounterType.Enemy) continue;
            VisualElement mapIcon = new VisualElement();
            mapIcon.AddToClassList("map-icon-base-style");

            // if we've already complete the encounter than it should be light gray
            if (IsEliteEncounter(encounter))
            {
                if (currentEncounterIndex == i)
                {
                    mapIcon.AddToClassList("active-elite-encounter");
                    activeEncounterIcon = mapIcon;
                }
                else
                {
                    mapIcon.AddToClassList("upcoming-elite-encounter");
                }
            }
            else if (i < currentEncounterIndex)
            {
                mapIcon.AddToClassList("map-icon-completed");
            }
            else if (currentEncounterIndex == i)
            {
                mapIcon.AddToClassList("map-icon-active");
                activeEncounterIcon = mapIcon;
            }
            else
            {
                mapIcon.AddToClassList("map-icon-upcoming");
            }

            mapIconContainer.Add(mapIcon);
            // PositionMapIcon(mapIcon, i - startIndex, endIndex - startIndex);
        }
        PopulateActAndEncounterNumbers();
        if (encounterType == EncounterType.Shop) mainTextLabel.text = "Shop";

        return container.Children().First();
    }

    private void PositionMapIconsAndArrow() {
        List<VisualElement> mapIcons = mapIconContainer.Children().ToList();

        float w = mapIconContainer.resolvedStyle.width;
        float h = mapIconContainer.resolvedStyle.height;

        Vector2 center = new Vector2(w * 0.5f, -h * 0.8f);

        float radius = h * 1.45f;

        float originDeg = 90f;
        float offset = 40f;
        float startDeg = originDeg + offset;
        float endDeg = originDeg - offset;

        for (int i = 0; i < mapIcons.Count; i++) {
            VisualElement mapIcon = mapIcons[i];

            float t = (mapIcons.Count == 1) ? 0.5f : (float)i / (mapIcons.Count - 1);
            float deg = Mathf.Lerp(startDeg, endDeg, t);
            float rad = deg * Mathf.Deg2Rad;

            float iconw = mapIcon.resolvedStyle.width;
            float iconh = mapIcon.resolvedStyle.height;

            float x = center.x + Mathf.Cos(rad) * radius - iconw * 0.5f;
            float y = center.y + Mathf.Sin(rad) * radius - iconh * 0.5f;

            mapIcon.style.left = x;
            mapIcon.style.top = y;
        }

        mapContainer.schedule.Execute(PositionArrow).ExecuteLater(0);
    }

    private void PositionArrow() {
        Vector2 target = activeEncounterIcon.worldBound.center;
        Vector2 pivot = VisualElementUtils.GetWorldPivot(arrow);
        Vector2 direction = target - pivot;
        float arrowDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowDeg -= 90f;

        arrow.style.rotate = new Rotate(new Angle(arrowDeg, AngleUnit.Degree));
    }

    private void PopulateActAndEncounterNumbers()
    {
        int temp = currentEncounterIndex - 1;
        int combatsFoughtThisFloor = 1;
        int elitesFought = 1;
        bool foundElite = false;
        while (temp >= 0)
        {
            Encounter encounter = map.encounters[temp];
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
        actEncounterNumberLabel.text = elitesFought.ToString() + "–" + combatsFoughtThisFloor.ToString();
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

    private void UpdateIconPositions(GeometryChangedEvent evt) {
        mapIconContainer.UnregisterCallback<GeometryChangedEvent>(UpdateIconPositions);
        PositionMapIconsAndArrow();
    }

    public void UpdateTurnCounter(int number) {
        mainTextLabel.text = "Turn " +number.ToString();
    }

    public void UpdateMoneyAmount(int money) {
        moneyLabel.text = money.ToString();
    }
}

/*

using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

public class MapView
{
    public VisualElement mapContainer;
    // TODO, could require the stylesheet in the constructor and fetch these from there
    public Color activeColor = Color.green;
    private List<Encounter> encounterList;

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


*/