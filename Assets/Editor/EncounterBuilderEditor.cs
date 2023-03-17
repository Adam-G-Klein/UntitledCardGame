using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EncounterBuilder))]
[CanEditMultipleObjects]
public class EncounterBuilderEditor : Editor {
    public EncounterVariableSO encounterToSet;

    public override void OnInspectorGUI() {
        EncounterBuilder encounterBuilder = (EncounterBuilder) serializedObject.targetObject;
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Test Utilities");
        EditorGUILayout.Space(5);

        encounterToSet = EditorGUILayout.ObjectField(
            encounterToSet,
            typeof(EncounterVariableSO),
            false) as EncounterVariableSO;
        
        if (GUILayout.Button("Set Active Encounter")) {
            if (encounterToSet != null)
                encounterBuilder.activeEncounter.SetValue(encounterToSet);
            else
                Debug.LogWarning("Must set encounter to set first!");
            
        }
    }
}
