using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstantiatePrefab : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private GameObject prefabToInstantiate;
    [SerializeField]
    private float scale = 5.0f;

    public InstantiatePrefab() {
        effectStepName = "InstantiatePrefab";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<GameObject> objects = document.GetGameObjects(inputKey);
        List<GameObject> distinctObjects = objects.Distinct().ToList();
        
        foreach (GameObject obj in distinctObjects) {
            Vector3 location = obj.transform.position;
            GameObject gameObject = GameObject.Instantiate(
                prefabToInstantiate,
                location,
                Quaternion.identity);
            gameObject.transform.localScale *= scale;
            foreach (Transform child in gameObject.transform) {
                child.localScale *= scale;
            }
        }

        yield return null;
    }
}
