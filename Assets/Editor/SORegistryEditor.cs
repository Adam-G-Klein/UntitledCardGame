#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

[CustomEditor(typeof(SORegistry))]
public class SORegistryEditor : Editor
{
    string guidField;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Auto-Populate"))
        {
            SORegistry registry = (SORegistry)target;
            PopulateSORegistry(registry);
        }

        if (GUILayout.Button("Print GUIDS"))
        {
            SORegistry registry = (SORegistry)target;
            List<IdentifiableSO> assets = registry.GetAllAssets();
            foreach (IdentifiableSO so in assets)
            {
                Debug.Log(so.GUID);
            }
        }

        if (GUILayout.Button("Check for duplicate GUIDS"))
        {
            SORegistry registry = (SORegistry)target;
            List<IdentifiableSO> assets = registry.GetAllAssets();
            HashSet<string> uuids = new HashSet<string>();
            foreach (IdentifiableSO so in assets)
            {
                if (uuids.Contains(so.GUID))
                {
                    Debug.LogError(String.Format("Found duplicate GUID: {0}", so.GUID));
                }
                else
                {
                    uuids.Add(so.GUID);
                }
            }
        }
        
        if (GUILayout.Button("Dedupe GUIDS"))
        {
            SORegistry registry = (SORegistry)target;
            List<IdentifiableSO> assets = registry.GetAllAssets();
            foreach (IdentifiableSO so in assets)
            {
                if (so != null) UnityEditor.EditorUtility.SetDirty(so);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        guidField = EditorGUILayout.TextField("GUID", guidField);
        if (GUILayout.Button("Find SO With GUID"))
        {
            SORegistry registry = (SORegistry)target;
            foreach (IdentifiableSO so in registry.GetAllAssets()) {
                if (so.GUID == guidField) {
                    Debug.Log(so);
                }
            }
        }
    }

    public static void PopulateSORegistry(SORegistry registry) {
        List<IdentifiableSO> allAssets = AssetDatabase.FindAssets("t:IdentifiableSO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<IdentifiableSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => asset != null)
            .ToList();

        registry.RegisterAssets(allAssets);

        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
    }
}
#endif