using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EncounterVariableSO))]
public class EncounterVariableSOEditor : Editor {
    EncounterType encounterType;

    public override void OnInspectorGUI() {
        EncounterVariableSO encounterVariable = (EncounterVariableSO) target;
        
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Encounter Variable Controls");
        EditorGUILayout.Space(5);

        encounterType = (EncounterType) EditorGUILayout.EnumPopup("Encounter Type:", encounterType);

        if (GUILayout.Button("Set Encounter")) {
            switch(encounterType) {
                case EncounterType.Enemy:
                    encounterVariable.encounter = new EnemyEncounter();
                break;
                
                case EncounterType.Shop:
                    encounterVariable.encounter = new ShopEncounter();
                break;

                default:
                    Debug.LogError("Unknown or not set encounter type");
                break;
            }
        }
    }
}
