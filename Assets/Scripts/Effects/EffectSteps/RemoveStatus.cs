using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveStatus : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private StatusEffect statusEffect;
    [SerializeField]
    private int scale = 0;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";
    [SerializeField]
    private string outputKey = "";

    public RemoveStatus() {
        effectStepName = "RemoveStatus";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> combatInstances = document.map.GetList<CombatInstance>(inputKey);
        if (combatInstances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }

        int finalScale = scale;
        if (getScaleFromKey) {
            finalScale = document.intMap[inputScaleKey];
        }

        int totalRemovedAmount = 0;
        foreach (CombatInstance instance in combatInstances) {
            int currentStatusCount = instance.statusEffects[statusEffect];
            int removedAmount = finalScale;
            if (currentStatusCount < finalScale) {
                removedAmount = currentStatusCount;
            }

            // Using ApplyStatusEffects function here in case we do an update based
            // status effect display in the future
            instance.ApplyStatusEffects(statusEffect, -removedAmount);
            totalRemovedAmount += removedAmount;
        }

        document.intMap[outputKey] = totalRemovedAmount;

        yield return null;
    }
}
