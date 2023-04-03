using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomVariableSO))]
public class RoomVariableSOEditor : Editor {
    // EncounterType encounterType;
    // EncounterVariableSO encounterVariable;
    // Encounter encounter;

    // public override void OnInspectorGUI() {
    //     RoomVariableSO roomVariable = (RoomVariableSO) target;
        
    //     DrawDefaultInspector();

    //     EditorGUILayout.Space(20);
    //     EditorGUILayout.LabelField("Room Variable Controls");
    //     EditorGUILayout.Space(5);

    //     encounterType = (EncounterType) EditorGUILayout.EnumPopup("Encounter Type:", encounterType);

    //     if (GUILayout.Button("Add Encounter to Room")) {
    //         switch(encounterType) {
    //             case EncounterType.Enemy:
    //                 encounter = new EnemyEncounter();
    //                 encounterReference = new EncounterReference(encounter);
    //                 encounterReference.ConstantValue = encounter;
    //             break;
                
    //             case EncounterType.Shop:
    //                 encounter = new ShopEncounter();
    //                 encounterReference = new EncounterReference(encounter);
    //                 encounterReference.ConstantValue = encounter;
    //             break;

    //             default:
    //                 Debug.LogError("Unknown or not set encounter type");
    //             break;
    //         }
    //         roomVariable.GetValue().encounters.Add(encounterReference);
    //     }
    // }
}
