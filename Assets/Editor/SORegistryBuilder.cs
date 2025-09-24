using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ScriptableObjectRegistryBuilder : IPreprocessBuildWithReport {
    // Order determines when it runs compared to other preprocessors
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        Debug.Log("Regenerating ScriptableObject Registry before build...");
        GenerateRegistry();
    }

    private void GenerateRegistry() {
        try {
            SORegistry registry = AssetDatabase.FindAssets("t:SORegistry")
                .Select(guid => AssetDatabase.LoadAssetAtPath<SORegistry>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .First();
            SORegistryEditor.PopulateSORegistry(registry);
        } catch (InvalidOperationException e) {
            Debug.LogError("Missing SO Registry. " + e.Message);
        }
    }
}