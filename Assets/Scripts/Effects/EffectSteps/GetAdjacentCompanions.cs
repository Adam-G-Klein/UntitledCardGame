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
        List<CompanionInstance> allCompanions = CombatEntityManager.Instance.getCompanions();
        foreach (CompanionInstance instance in companionInstances) {
            adjacentCompanions.AddRange(GetAdjacentCompanionns(instance, allCompanions));
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

    private List<CompanionInstance> GetAdjacentCompanionns(CompanionInstance instance, List<CompanionInstance> allCompanions) {
        List<CompanionInstance> adjacentCompanions = new List<CompanionInstance>();
        int companionIndex = allCompanions.IndexOf(instance);
        // Debug.Log("Dingo");
        Debug.Log(companionIndex);
        Debug.Log(allCompanions.Count);

        // Check if for some reason the companion chosen isn't in the list of all companions
        // Also check if only one single companion in the list
        if (companionIndex == -1 || allCompanions.Count == 1) {
            return adjacentCompanions;
        }

        // Check if the companion is in the back (0 index)
        if (companionIndex == 0) {
            adjacentCompanions.Add(allCompanions[1]);
            return adjacentCompanions;
        }

        // Check if the companion is at the front (Count - 1 index)
        if (companionIndex == allCompanions.Count - 1) {
            adjacentCompanions.Add(allCompanions[companionIndex - 1]);
            return adjacentCompanions;
        }

        adjacentCompanions.Add(allCompanions[companionIndex - 1]);
        adjacentCompanions.Add(allCompanions[companionIndex + 1]);
        return adjacentCompanions;
    }
}