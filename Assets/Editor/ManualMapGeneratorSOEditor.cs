using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ManualMapGeneratorSO))]
public class ManualMapGeneratorSOEditor : Editor {
    EncounterType encounterType;

    public override void OnInspectorGUI() {
        ManualMapGeneratorSO mapGenerator = (ManualMapGeneratorSO) target;
        
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Manual Map Generator Controls");
        EditorGUILayout.Space(5);

        encounterType = (EncounterType) EditorGUILayout.EnumPopup("Encounter Type:", encounterType);

        if (GUILayout.Button("Add Encounter to Map")) {
            switch(encounterType) {
                case EncounterType.Enemy:
                    mapGenerator.map.encounters.Add(new EnemyEncounter());
                break;
                
                case EncounterType.Shop:
                    mapGenerator.map.encounters.Add(new ShopEncounter());
                break;

                default:
                    Debug.LogError("Unknown or not set encounter type");
                break;
            }
        }
    }
}
