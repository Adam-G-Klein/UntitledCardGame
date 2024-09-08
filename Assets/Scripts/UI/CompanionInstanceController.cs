using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionInstanceController : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;


    public void Awake() {
    }

    public List<CompanionInstance> SetupCompanions(List<Companion> companions, EncounterConstantsSO encounterConstants) {
        List<CompanionInstance> created = new();
        foreach (Companion companion in companions) {
            Debug.Log("instating companion " + companion.companionType.ToString());
            created.Add(PrefabInstantiator.InstantiateCompanion(
                encounterConstants.companionPrefab,
                companion,
                Vector3.zero,
                parentTransform
            ));
        }
        return created;
    }
}
