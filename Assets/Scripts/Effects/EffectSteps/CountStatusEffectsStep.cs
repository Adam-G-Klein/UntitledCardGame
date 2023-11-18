using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountStatusEffectsStep : EffectStep
{
    [SerializeField]
     private string inputKey = "";
    [SerializeField]
    private StatusEffect statusEffect;
    [SerializeField]
    private string outputKey = "";


    public CountStatusEffectsStep() {
        effectStepName = "CountStatusEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> combatInstances = document.map.GetList<CombatInstance>(inputKey);
        if (combatInstances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }
        
        int total = 0;
        foreach (CombatInstance instance in combatInstances) {
            total = total + instance.statusEffects[statusEffect];
        }

        document.intMap.Add(outputKey, total);
        yield return null;
    }
}
