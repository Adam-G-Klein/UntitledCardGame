using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatePrefab : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private GameObject prefabToInstantiate;

    public InstantiatePrefab() {
        effectStepName = "InstantiatePrefab";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<GameObject> objects = document.GetGameObjects(inputKey);
        
        foreach (GameObject obj in objects) {
            Vector3 location = obj.transform.position;
            GameObject.Instantiate(
                prefabToInstantiate,
                location,
                Quaternion.identity);
        }

        yield return null;
    }
}
