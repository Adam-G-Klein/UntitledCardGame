using UnityEngine;
using UnityEditor;
using System.Linq;
using PlasticPipe.PlasticProtocol.Messages;

public class EditorRuntimeTools : EditorWindow
{
    private PlayerDataVariableSO[] allPlayerDataAssets;
    private Vector2 scrollPos;

    [MenuItem("Window/Editor Runtime Tools")]
    public static void ShowWindow()
    {
        GetWindow<EditorRuntimeTools>("Editor Runtime Tools");
    }

    private void OnEnable()
    {
        RefreshAssets();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
            RefreshAssets();
    }

    private void RefreshAssets()
    {
        var guids = AssetDatabase.FindAssets("t:PlayerDataVariableSO");
        allPlayerDataAssets = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<PlayerDataVariableSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => asset != null)
            .ToArray();
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use this tool.", MessageType.Info);
            return;
        }

        if (allPlayerDataAssets == null || allPlayerDataAssets.Length == 0)
        {
            EditorGUILayout.HelpBox("No PlayerData assets found.", MessageType.Warning);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var data in allPlayerDataAssets)
        {
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField($"{data.name} (Gold: {data.GetValue().gold})", GUILayout.Width(200));

            if (GUILayout.Button("Give 999 Gold"))
            {
                data.GetValue().gold = 999;
                Debug.Log($"Gave 999 gold to {data.name}");
                EditorUtility.SetDirty(data); // optional: shows changes in inspector
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}
