using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class IdentifiableSO : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string guid;

    public string GUID => guid;

#if UNITY_EDITOR
    private static HashSet<string> existingUuids = new HashSet<string>();
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
        {
            AssignNewGUID();
        }
    }

    public void AssignNewGUID()
    {
        guid = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}