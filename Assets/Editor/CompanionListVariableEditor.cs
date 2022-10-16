using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CompanionListVariable))]
public class CompanionListVariableEditor : Editor {
    private CompanionTypeSO companionType = null;

    public override void OnInspectorGUI() {
        CompanionListVariable companionListVariable = (CompanionListVariable) target;
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Test Utilities");
        EditorGUILayout.Space(5);

        companionType = EditorGUILayout.ObjectField(
            companionType,
            typeof(CompanionTypeSO),
            false) as CompanionTypeSO;
        
        if (GUILayout.Button("Add Companion")) {
            if (companionType != null)
                companionListVariable.companionList.Add(new Companion(companionType));
            else
                Debug.LogWarning("Must set companion type first!");
        }
    }
}
