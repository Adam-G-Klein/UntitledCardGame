using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabOnStart : MonoBehaviour
{
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.Instantiate(prefab);
    }
}
