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

public class MapView {
    public VisualElement mapContainer;
    // TODO, could require the stylesheet in the constructor and fetch these from there
    public Color activeColor = Color.green;
    private List<Encounter> encounterList = new ();

    public MapView(IEncounterBuilder encounterBuilder) {
        if (encounterBuilder is EnemyEncounterManager) encounterList = EnemyEncounterManager.Instance.gameState.map.GetValue().encounters;
        if (encounterBuilder is ShopManager) encounterList = ShopManager.Instance.gameState.map.GetValue().encounters;
        mapContainer = makeMapView(encounterBuilder);
    }

    private VisualElement makeMapView(IEncounterBuilder encounterBuilder) {
        // get current sceneIndex from gameState somehow;
        int curEncounterIndex = 0;
        if (encounterBuilder is EnemyEncounterManager) curEncounterIndex = EnemyEncounterManager.Instance.gameState.currentEncounterIndex;
        if (encounterBuilder is ShopManager) curEncounterIndex = ShopManager.Instance.gameState.currentEncounterIndex;
        Debug.LogError(curEncounterIndex);
        VisualElement container = new VisualElement();
        container.AddToClassList("map-container");

        for (int i = 0; i < 6; i++) {
            VisualElement mapSection = new VisualElement();
            mapSection.AddToClassList("map-section");
            VisualElement mapIcon = new VisualElement();
            mapIcon.AddToClassList("map-symbol");
            
            Sprite texture = null;
            if (curEncounterIndex + i < encounterList.Count()) {
                Encounter encounter = encounterList[curEncounterIndex + i];
                if (encounter.getEncounterType() == EncounterType.Enemy) {
                    EnemyEncounter EE = (EnemyEncounter) encounter;
                    if (EE.isEliteEncounter) {
                        Sprite sprite = EE.enemyList[0].enemyType.sprite;
                        texture = sprite;
                    } else {
                        texture = Resources.Load<Sprite>("enemySymbol");
                    }
                } else {
                    texture = Resources.Load<Sprite>("shopSymbol");
                }
            }

            if (i == 0) {
                mapIcon.style.unityBackgroundImageTintColor = new StyleColor(new Color(0, 1, 0, 1));
            }
            
            if (texture == null) {
                mapIcon.visible = false;
            } else {
                mapIcon.style.backgroundImage = new StyleBackground(texture);
            }

            mapSection.Add(mapIcon);
            container.Add(mapSection);
        }
        Debug.LogError(container.Children().Count());
        Debug.LogError("here be the map view sire");
        return container;
    }
}
