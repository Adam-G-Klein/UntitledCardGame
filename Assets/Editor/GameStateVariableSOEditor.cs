using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameStateVariableSO))]
public class GameStateVariableSOEditor : Editor {

    public override void OnInspectorGUI() {
        GameStateVariableSO gameStateVariableSO = (GameStateVariableSO) target;
        DrawDefaultInspector();
        
        if (GUILayout.Button("Save For Building")) {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(gameStateVariableSO);
            AssetDatabase.SaveAssets();
        }
    }
}
