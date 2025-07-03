using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;

public class FXEffectStep: EffectStep {

    [SerializeField]
    private bool waitForEffect = false;
    [SerializeField]
    private GameObject fXExperiencePrefab;
    [Header("If not specified, will default to 0,0")]
    [SerializeField]
    private string rootLocationGameObjectKey = "";
    [SerializeField]
    private List<Mapping> positionMappingList = new List<Mapping>();
    [SerializeField]
    private List<Mapping> gameobjectMappingList = new List<Mapping>();

    private bool isEffectCompleted;

    public FXEffectStep() {
        effectStepName = "FXEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        Vector3 rootLocation = Vector3.zero;
        try {
            List<GameObject> rootLocationGameObjects = document.map.GetList<GameObject>(rootLocationGameObjectKey);
            if (rootLocationGameObjects.Count > 1) {
                EffectError(String.Format("Can't set root FXExpreience location because {0} were found", rootLocationGameObjects.Count));
            } else if (rootLocationGameObjects.Count == 1) {
                rootLocation = rootLocationGameObjects[0].transform.position;
            }
        } catch (Exception e) {
            // I know this is terrible, but I don't want to go refactor the
            // effect document map code at the moment
        }

        FXExperience experience = PrefabInstantiator.instantiateFXExperience(fXExperiencePrefab, rootLocation);

        foreach (Mapping posMapping in positionMappingList) {
            List<GameObject> gameObjects = document.map.GetList<GameObject>(posMapping.fromWorkflow);
            if (gameObjects.Count != 1) {
                EffectError(String.Format("Can't bind map object to FXExperience because there were {0} options", gameObjects.Count));
                yield break;
            }
            experience.AddLocationToKey(posMapping.toFxExperience, gameObjects[0].transform.position);
        }

        Dictionary<String, GameObject> keyToGameObjectDict = new Dictionary<String, GameObject>();
        foreach (Mapping goMapping in gameobjectMappingList) {
            List<GameObject> gameObjects = document.map.GetList<GameObject>(goMapping.fromWorkflow);
            if (gameObjects.Count != 1) {
                EffectError(String.Format("Can't bind map object to FXExperience because there were {0} options", gameObjects.Count));
                yield break;
            }
            keyToGameObjectDict.Add(goMapping.toFxExperience, gameObjects[0]);
        }
        experience.BindGameObjectsToTracks(keyToGameObjectDict);

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