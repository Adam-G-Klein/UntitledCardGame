using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterEntityByStatus : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private StatusEffect status;
    [SerializeField]
    private string outputKey = "";

    public FilterEntityByStatus() {
        effectStepName = "FilterEntityByStatus";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> combatInstances = document.map.GetList<CombatInstance>(inputKey);
        if (combatInstances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }
        List<CombatInstance> filteredList = new List<CombatInstance>();
        foreach (CombatInstance instance in combatInstances) {
            if (instance.statusEffects[status] > 0) {
                filteredList.Add(instance);
            }
        }
        document.map.AddItems<CombatInstance>(outputKey, filteredList);
        yield return null;
    }
}
