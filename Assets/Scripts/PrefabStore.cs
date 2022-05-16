using UnityEngine;
using System;

public class PrefabStore : MonoBehaviour
{
    [SerializeField]
    public GameObject[] prefabs;

    public GameObject getPrefabByName(string name)
    {
        foreach(GameObject prefab in prefabs){
            if(prefab.name.Equals(name)){
                return prefab;
            }
        }
        Debug.LogException(new InvalidOperationException("No prefab by name " + name + " in prefab store"), this);
        return null;
    }
}
