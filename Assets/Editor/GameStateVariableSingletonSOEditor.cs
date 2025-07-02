using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameStateVariableSingletonSO))]
public class GameStateVariableSingletonSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var settings = GameStateVariableSingletonSO.instance;

        // Save button
        if (GUILayout.Button("Save Settings to Disk"))
        {
            settings.SaveSettings();
        }
    }
}