#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

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
            List<IdentifiableSO> allAssets = AssetDatabase.FindAssets("t:IdentifiableSO")
                .Select(guid => AssetDatabase.LoadAssetAtPath<IdentifiableSO>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToList();

            registry.RegisterAssets(allAssets);

            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }

        if (GUILayout.Button("Print GUIDS"))
        {
            SORegistry registry = (SORegistry)target;
            List<IdentifiableSO> assets = registry.GetAllAssets();
            foreach (IdentifiableSO so in assets) {
                Debug.Log(so.GUID);
            }
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
}
#endif