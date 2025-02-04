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
    private List<string> encounterList = new() {
        "Combat", "Shop", "Combat", "Shop", "Combat", "Shop", "Combat", "Shop", "Elite", "Shop",
        "Combat", "Shop", "Combat", "Shop", "Combat", "Shop", "Combat", "Shop", "Elite", "Shop",
        "Combat", "Shop", "Combat", "Shop", "Combat", "Shop", "Combat", "Shop", "Boss", "", "", "", ""};

    public MapView(IEncounterBuilder encounterBuilder) {
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
            string encounterType = encounterList[curEncounterIndex + i];
            Sprite texture = null;
            if (encounterType == "Combat") {
                texture = Resources.Load<Sprite>("enemySymbol");
            } else if (encounterType == "Shop") {
                texture = Resources.Load<Sprite>("shopSymbol");
            } else if (encounterType == "Elite") {
                texture = Resources.Load<Sprite>("eliteSymbol");
            } else if (encounterType == "Boss") {
                texture = Resources.Load<Sprite>("bossSymbol");
            }

            if (i == 0) {
                mapIcon.style.unityBackgroundImageTintColor = new StyleColor(new Color(0, 1, 0, 1));
            }
            
            if (texture != null) {
                mapIcon.style.backgroundImage = new StyleBackground(texture);
            } else {
                mapIcon.visible = false;
            }

            mapSection.Add(mapIcon);
            container.Add(mapSection);
        }
        Debug.LogError(container.Children().Count());
        Debug.LogError("here be the map view sire");
        return container;
    }
}
