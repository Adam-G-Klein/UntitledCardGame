using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CompanionListVariableSO))]
public class CompanionListVariableEditor : Editor {
    private CompanionTypeSO companionType = null;

    public override void OnInspectorGUI() {
        CompanionListVariableSO companionListVariable = (CompanionListVariableSO) target;
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
            
            // These three calls cause the asset to actually be modified
            // on disc when we hit the button
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(companionListVariable);
            AssetDatabase.SaveAssets();
        }
    }
}