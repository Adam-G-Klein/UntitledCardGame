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
        List<TargettableEntity> entities = document.getTargettableEntities(inputKey);
        
        foreach (TargettableEntity entity in entities) {
            Vector3 location = entity.transform.position;
            GameObject.Instantiate(
                prefabToInstantiate,
                location,
                Quaternion.identity);
        }

        yield return null;
    }
}
