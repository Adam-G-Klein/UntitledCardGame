using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EncounterManager))]
[CanEditMultipleObjects]
public class EncounterBuilderEditor : Editor {
    public EncounterVariableSO encounterToSet;

    public override void OnInspectorGUI() {
        EncounterManager encounterManager = (EncounterManager) serializedObject.targetObject;
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
                encounterManager.activeEncounterVariable.SetValue(encounterToSet);
            else
                Debug.LogWarning("Must set encounter to set first!");
            
        }
    }
}
