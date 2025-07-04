using UnityEngine;

public abstract class IdentifiableSO : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string guid;

    public string GUID => guid;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}