#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

public class IdentifiableSOProcessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        bool modified = false;
        foreach (var path in imported)
        {
            var obj = AssetDatabase.LoadAssetAtPath<IdentifiableSO>(path);
            if (obj && !string.IsNullOrEmpty(obj.GUID))
            {
                // Check for duplicates in project
                var matches = AssetDatabase.FindAssets("t:" + obj.GetType().Name)
                    .Select(g => AssetDatabase.LoadAssetAtPath<IdentifiableSO>(AssetDatabase.GUIDToAssetPath(g)))
                    .Where(a => a.GUID == obj.GUID)
                    .ToList();

                if (matches.Count > 1)
                {
                    Debug.LogWarning("IdentifableSOProcessor found duplicate GUIDs on ScriptableObjects, deduping.");
                    obj.AssignNewGUID();
                    EditorUtility.SetDirty(obj);
                    modified = true;
                }
            }
        }
        if (modified)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif