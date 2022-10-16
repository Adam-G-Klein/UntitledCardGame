using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IntGameEvent))]
public class IntGameEventEditor : Editor
{
    private int intValue = 0;

    public override void OnInspectorGUI() {
        IntGameEvent intEvent = (IntGameEvent) target;
        DrawDefaultInspector();

        intValue = EditorGUILayout.IntField("Int Value", intValue);
        
        if (GUILayout.Button("Test Raise Event")) {
            intEvent.Raise(intValue);
        }
    }
}
