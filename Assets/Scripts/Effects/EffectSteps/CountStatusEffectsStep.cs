using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountStatusEffectsStep : EffectStep, IEffectStepCalculation
{
    [SerializeField]
     private string inputKey = "";
    [SerializeField]
    private StatusEffectType statusEffect;
    [SerializeField]
    private bool onlyCountStatusOnce = false;
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
            if (onlyCountStatusOnce && instance.GetStatus(statusEffect) > 0) {
                total = total + 1;
            } else if (!onlyCountStatusOnce) {
                total = total + instance.GetStatus(statusEffect);
            }
        }

        document.intMap.Add(outputKey, total);
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}
