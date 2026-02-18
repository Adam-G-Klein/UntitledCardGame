using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Drawing.Printing;
using Codice.CM.Client.Gui;

[CustomEditor(typeof(GameStateVariableSO))]
public class GameStateVariableSOEditor : Editor {
    private EncounterType encounterType;
    private int mapIndex = -1;
    private MapGeneratorSO mapGenerator;
    private Location location;

    public override void OnInspectorGUI() {
        GameStateVariableSO gameStateVariableSO = (GameStateVariableSO) target;
        DrawDefaultInspector();

        EditorGUILayout.Space(40);
        EditorGUILayout.LabelField("Developer Console");

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Current Location Controls");
        EditorGUILayout.Space(10);

        location = (Location) EditorGUILayout.EnumPopup("Location:", location);
        if(GUILayout.Button("Set Location")) {
            gameStateVariableSO.currentLocation = location;
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Companion List Controls");
        EditorGUILayout.Space(10);
        if(GUILayout.Button("Heal Companions")) {
            foreach(Companion companion in gameStateVariableSO.companions.activeCompanions) {
                companion.combatStats.currentHealth = companion.combatStats.maxHealth;
            }
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Active Encounter Controls");
        EditorGUILayout.Space(10);
        encounterType = (EncounterType) EditorGUILayout.EnumPopup("Encounter Type (set index to -1 to use):", encounterType);
        EditorGUILayout.Space(2);
        GUIStyle textStyle = EditorStyles.label;
        textStyle.wordWrap = true;
        string text = "Optional: Designate which encounter to use by its mapIndex.\nDoes require figuring out which mapIndex has the encounter type you're looking for.\n OR Set to -1 to use the map's first encounter of the above type.";
        Rect textRect = GUILayoutUtility.GetRect(new GUIContent(text), textStyle);
        EditorGUI.LabelField(textRect, text, textStyle);
        mapIndex = EditorGUILayout.IntField("Map Index:", mapIndex);

        if (GUILayout.Button("Set Active Encounter")) {
            Encounter encounter;
            if(mapIndex != -1) {
                if(mapIndex < 0 || mapIndex >= gameStateVariableSO.map.GetValue().encounters.Count) {
                    Debug.LogError("Map index is not -1, attempted to find the designated encounter. Error: Map index must be between 0 and " + (gameStateVariableSO.map.GetValue().encounters.Count - 1) + " (the number of encounters in the current map) inclusive.");
                }
                encounter = gameStateVariableSO.map.GetValue().encounters[mapIndex];
                if(encounter == null) {
                    Debug.LogError("No encounter found at map index " + mapIndex + " in the current map.");
                }
                setNextEncounterAndMapIndex(gameStateVariableSO, encounter, mapIndex);
            }
            else {
                // so that we don't change the editor tool's value
                int tempMapIndex;
                (encounter, tempMapIndex) = getFirstEncounterOfTypeInMap(encounterType, gameStateVariableSO.map.GetValue());
                if(encounter == null) {
                    Debug.LogError("No encounter of type " + encounterType + " found in the current map.");
                }
                setNextEncounterAndMapIndex(gameStateVariableSO, encounter, tempMapIndex);
            }
            gameStateVariableSO.activeEncounter.SetValue(encounter);

            switch(encounter.ToLocation()) {
                case Location.SHOP:
                    gameStateVariableSO.currentLocation = Location.SHOP;
                    break;
                case Location.COMBAT:
                    gameStateVariableSO.currentLocation = Location.COMBAT;
                    break;
                default:
                    Debug.LogError("ERROR: Invalid location in Set Active Encounter switch case");
                    break;
            }
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Map controls");
        EditorGUILayout.Space(10);

        mapGenerator = EditorGUILayout.ObjectField(
            mapGenerator,
            typeof(MapGeneratorSO),
            false) as MapGeneratorSO;
        if (GUILayout.Button("Regenerate Map, resetting all encounters")) {
            gameStateVariableSO.map.SetValue(mapGenerator.generateMap());
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Player Data Controls");
        EditorGUILayout.Space(10);

        if(GUILayout.Button("Reset Player Data")) {
            gameStateVariableSO.playerData.initialize(gameStateVariableSO.baseShopData);
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Other");
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Save For Building")) {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(gameStateVariableSO);
            AssetDatabase.SaveAssets();
        }
    }

    private (Encounter, int) getFirstEncounterOfTypeInMap(EncounterType encounterType, Map map) {
        for(int i = 0; i < map.encounters.Count; i++) {
            if(map.encounters[i].getEncounterType() == encounterType) {
                return (map.encounters[i], i);
            }
        }
        return (null, -1);
    }

    private void setNextEncounterAndMapIndex(GameStateVariableSO gameStateVariableSO, Encounter encounter, int mapIndex) {
        if(mapIndex != -1 && mapIndex < gameStateVariableSO.map.GetValue().encounters.Count - 1) {
            gameStateVariableSO.nextEncounter.SetValue(encounter);
            gameStateVariableSO.nextMapIndex = mapIndex + 1;
            gameStateVariableSO.currentEncounterIndex = mapIndex;
        }
        else {
            gameStateVariableSO.nextEncounter.SetValue(gameStateVariableSO.map.GetValue().encounters[0]);
            gameStateVariableSO.nextMapIndex = 0;
            gameStateVariableSO.currentEncounterIndex = mapIndex;
        }
    }
}
