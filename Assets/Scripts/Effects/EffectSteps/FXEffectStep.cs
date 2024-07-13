using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FXEffectStep: EffectStep {

    [SerializeField]
    private bool waitForEffect = true;
    [SerializeField]
    private GameObject fXExperiencePrefab;
    [SerializeField]
    private List<Mapping> keyMappingList = new List<Mapping>();

    private bool isEffectCompleted;

    public FXEffectStep() {
        effectStepName = "FXEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        FXExperience experience = PrefabInstantiator.instantiateFXExperience(fXExperiencePrefab, Vector2.zero);

        foreach (Mapping keyMapping in keyMappingList) {
            List<GameObject> gameObjects = document.map.GetList<GameObject>(keyMapping.fromWorkflow);
            if (gameObjects.Count != 1) {
                EffectError(String.Format("Can't bind map object to FXExperience because there were {0} options", gameObjects.Count));
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