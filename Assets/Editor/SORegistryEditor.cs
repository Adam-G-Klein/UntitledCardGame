#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(SORegistry))]
public class SORegistryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Auto-Populate"))
        {
            SORegistry registry = (SORegistry)target;
            List<IdentifiableSO> allAssets = AssetDatabase.FindAssets("t:IdentifiableSO")
                .Select(guid => AssetDatabase.LoadAssetAtPath<IdentifiableSO>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToList();

            registry.RegisterAssets(allAssets);

            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif