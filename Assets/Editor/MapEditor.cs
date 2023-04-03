using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapVariableSO))]
public class MapEditor : Editor {
    EncounterType encounterType;

    public override void OnInspectorGUI() {
        MapVariableSO mapVariable = (MapVariableSO) target;
        
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Map Controls");
        EditorGUILayout.Space(5);

        encounterType = (EncounterType) EditorGUILayout.EnumPopup("Encounter Type:", encounterType);

        if (GUILayout.Button("Add Encounter")) {
            switch(encounterType) {
                case EncounterType.Enemy:
                    mapVariable.GetValue().encounters.Add(new EnemyEncounter());
                break;
                
                case EncounterType.Shop:
                    mapVariable.GetValue().encounters.Add(new ShopEncounter());
                break;

                default:
                    Debug.LogError("Unknown or not set encounter type");
                break;
            }
        }
    }
}
