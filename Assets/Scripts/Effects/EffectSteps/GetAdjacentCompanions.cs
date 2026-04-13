using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetAdjacentCompanions : EffectStep, IEffectStepCalculation {

    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private bool includeInputCompanion = false;
    [SerializeField]
    private string outputKey = "";

    public GetAdjacentCompanions() {
        effectStepName = "GetAdjacentCompanions";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        List<CompanionInstance> companionInstances = document.map.GetList<CompanionInstance>(inputKey);
        if (companionInstances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield break;
        }

        List<CompanionInstance> adjacentCompanions = new List<CompanionInstance>();
        // List<CompanionInstance> allCompanions = CombatEntityManager.Instance.getCompanions();
        foreach (CompanionInstance instance in companionInstances) {
            adjacentCompanions.AddRange(CombatEntityManager.Instance.GetAdjacentCompanions(instance, false));
        }
        if (includeInputCompanion) {
            adjacentCompanions.AddRange(companionInstances);
        }

        EffectUtils.AddCompanionsToDocument(document, outputKey, adjacentCompanions);
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }

}