using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UIElements;

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
    [SerializeField]
    private List<Mapping> vePositionMappingList = new List<Mapping>();
    [SerializeField]
    private List<Mapping> veMappingList = new List<Mapping>();

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

        foreach (Mapping veMapping in veMappingList) {
            List<VisualElement> visualElements = document.map.GetList<VisualElement>(veMapping.fromWorkflow);
            if (visualElements.Count != 1) {
                EffectError(String.Format("Can't bind map VE to FXExperience because there were {0} options", visualElements.Count));
                yield break;
            }
            experience.AddVisualElementToKey(veMapping.toFxExperience, visualElements[0]);
        }

        foreach (Mapping vePosMapping in vePositionMappingList) {
            List<VisualElement> visualElements = document.map.GetList<VisualElement>(vePosMapping.fromWorkflow);
            if (visualElements.Count != 1) {
                EffectError(String.Format("Can't bind map VE to FXExperience because there were {0} options", visualElements.Count));
                yield break;
            }
            Rect veRect = visualElements[0].worldBound;
            Vector2 centerPos = new Vector2(veRect.xMin + (veRect.width / 2f), veRect.yMin + (veRect.height / 2f));
            experience.AddLocationToKey(vePosMapping.toFxExperience, centerPos);
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