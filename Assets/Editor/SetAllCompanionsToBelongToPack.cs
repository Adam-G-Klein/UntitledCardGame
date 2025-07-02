using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class SetAllCompanionsToBelongToPackWindow : EditorWindow
{
    private static PackSO targetPack;

    [MenuItem("Tools/Link CompanionTypes To PackSO")]
    public static void ShowWindow()
    {
        var window = GetWindow<SetAllCompanionsToBelongToPackWindow>("Companion to Pack Linker");
        window.minSize = new Vector2(400, 120);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Link CompanionTypeSOs to a Target PackSO", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetPack = EditorGUILayout.ObjectField("Target ScriptableObject", targetPack, typeof(PackSO), false) as PackSO;

        EditorGUILayout.Space();

        if (GUILayout.Button("Link All CompanionTypeSOs"))
        {
            if (targetPack == null)
            {
                Debug.LogError("Please assign a target PackSO.");
                return;
            }

            SetAllCompanionsToBelongToPack();
        }
    }

    public static void SetAllCompanionsToBelongToPack()
    {
        // Select a folder within the project
        string absoluteFolderPath = EditorUtility.OpenFolderPanel("Select Folder with Companion Types", Application.dataPath, "");
        if (string.IsNullOrEmpty(absoluteFolderPath))
            return;

        // Convert absolute path to relative project path
        string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6); // remove "Assets/"
        if (!absoluteFolderPath.StartsWith(projectPath))
        {
            Debug.LogError("Selected folder is outside the project.");
            return;
        }
        string relativeFolderPath = absoluteFolderPath.Substring(projectPath.Length);
        Debug.Log("Searching in folder " + relativeFolderPath);

        // Find all ScriptableObject assets of type CompanionTypeSO
        string[] guids = AssetDatabase.FindAssets("t:CompanionTypeSO");
        List<CompanionTypeSO> companionTypes = new List<CompanionTypeSO>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CompanionTypeSO obj = AssetDatabase.LoadAssetAtPath<CompanionTypeSO>(assetPath);
            if (!assetPath.StartsWith(relativeFolderPath))
            {
                continue;
            }
            if (obj != null)
                companionTypes.Add(obj);
        }
        foreach (CompanionTypeSO c in companionTypes)
        {
            Debug.Log($"Linking {c.name} to {targetPack.name}");

            // Assign the reference (change field name to match your actual field)
            c.pack = targetPack;

            // Mark dirty and save
            EditorUtility.SetDirty(c);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All CompanionTypeSOs updated successfully.");

    }
}
