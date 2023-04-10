using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapVariableSO))]
public class MapVariableSOEditor : Editor {

    public override void OnInspectorGUI() {
        MapVariableSO mapVariable= (MapVariableSO) target;
        Map map = mapVariable.GetValue();
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Test Utilities");
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Map Completion: " + map.getCompletionPercentage() + "%");

        if (GUILayout.Button("Reset Map Completion")) {
            map.resetCompletion();
        }
    }
}
