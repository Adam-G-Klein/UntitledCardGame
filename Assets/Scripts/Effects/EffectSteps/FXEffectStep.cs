using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;

public class FXEffectStep: EffectStep {

    [SerializeField]
    private bool waitForEffect = true;
    [SerializeField]
    private GameObject fXExperiencePrefab;
    [Header("If not specified, will default to 0,0")]
    [SerializeField]
    private string rootLocationGameObjectKey = "";
    [SerializeField]
    private List<Mapping> keyMappingList = new List<Mapping>();

    private bool isEffectCompleted;

    public FXEffectStep() {
        effectStepName = "FXEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        Vector3 rootLocation = Vector3.zero;
        List<GameObject> rootLocationGameObjects = document.map.GetList<GameObject>(rootLocationGameObjectKey);
        if (rootLocationGameObjects.Count > 1) {
            EffectError(String.Format("Can't set root FXExpreience location because {0} were found", rootLocationGameObjects.Count));
        } else if (rootLocationGameObjects.Count == 1) {
            rootLocation = rootLocationGameObjects[0].transform.position;
        }

        FXExperience experience = PrefabInstantiator.instantiateFXExperience(fXExperiencePrefab, rootLocation);

        foreach (Mapping keyMapping in keyMappingList) {
            List<GameObject> gameObjects = document.map.GetList<GameObject>(keyMapping.fromWorkflow);
            if (gameObjects.Count != 1) {
                EffectError(String.Format("Can't bind map object to FXExperience because there were {0} options", gameObjects.Count));
                yield break;
            }
            experience.AddLocationToKey(keyMapping.toFxExperience, gameObjects[0].transform.position);
        }

        if (waitForEffect) {
            isEffectCompleted = false;
            experience.onExperienceOver += EffectCompletedCallback;
            experience.StartExperience();
            yield return new WaitUntil(() => isEffectCompleted == true);
        } else {
            experience.StartExperience();
        }
        yield return null;
    }

    private void EffectCompletedCallback() {
        isEffectCompleted = true;
    }

    [System.Serializable]
    public class Mapping {
        public string fromWorkflow = "";
        public string toFxExperience = "";

        public Mapping(string fromWorkflow, string toFxExperience) {
            this.fromWorkflow = fromWorkflow;
            this.toFxExperience = toFxExperience;
        }
    }
}