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

    private float originDeg = 90f;
    private float offset = 40f;

    private Map map;
    private int currentEncounterIndex;
    private EncounterType encounterType;
    // Only gets set if the curernt encounter is an enemy encounter
    private VisualElement activeEncounterIcon;
    private bool isShopEncounter = false;
    // Only gets set if the current encounter is a shop encounter
    private VisualElement previousEncounterIcon;

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

            if (currentEncounterIndex - 1 == i) {
                previousEncounterIcon = mapIcon;
                isShopEncounter = true;
            }

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

        float startDeg = originDeg + offset;
        float endDeg = originDeg - offset;

        Vector2 center = new Vector2(w * 0.5f, -h * 0.8f);

        float radius = h * 1.45f;

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
        List<VisualElement> mapIcons = mapIconContainer.Children().ToList();

        Vector2 target;
        if (isShopEncounter) {
            Vector2 prevPos = previousEncounterIcon.worldBound.center;
            int prevIndex = mapIcons.IndexOf(previousEncounterIcon);
            Vector2 nextPos = mapIcons[prevIndex + 1].worldBound.center;
            target = prevPos + (nextPos - prevPos) * 0.5f;
        } else {
            target = activeEncounterIcon.worldBound.center;
        }
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

    public VisualElement GetMoneyDisplay() {
        return moneyLabel;
    }
}