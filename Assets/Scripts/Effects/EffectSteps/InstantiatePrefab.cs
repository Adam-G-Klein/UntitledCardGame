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

        // The caster, when this effect originates from a combat entity. Used to decide
        // whether the prefab should be preceded by a precast trail (ally->ally / enemy->enemy).
        CombatInstance caster = PrecastTrailController.GetCasterFromDocument(document);

        foreach (GameObject obj in distinctObjects) {
            GameObject targetObj = obj;

            // Spawn exactly as before; captured so it can either fire now or when the trail lands.
            System.Action spawn = () => {
                if (targetObj == null) return;
                GameObject spawned = GameObject.Instantiate(
                    prefabToInstantiate,
                    targetObj.transform.position,
                    Quaternion.identity);
                spawned.transform.localScale *= scale;
                foreach (Transform child in spawned.transform) {
                    child.localScale *= scale;
                }
            };

            CombatInstance target = targetObj.GetComponent<CombatInstance>();
            if (PrecastTrailController.ShouldTrail(caster, target)) {
                // "Do something to another friendly": arc a trail from caster to target,
                // then play the payload prefab on arrival.
                PrecastTrailController.Instance.PlayTrailThenSpawn(caster, target, spawn);
            } else {
                spawn();
            }
        }

        yield return null;
    }
}
