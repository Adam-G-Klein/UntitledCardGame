using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that adds a status of some kind to an entity

    Input: Entity to add the status to
    Output: NA
    Parameters:
        - StatusEffect: The status effect to add to the entity
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class ApplyStatus : EffectStep, ITooltipProvider
{
    [SerializeField]
    [Tooltip(
        "Designates what entity to add status to based on " +
        "the key the entity was saved to from a previous effect"
    )]
    private string inputKey = "";
    [SerializeField]
    private StatusEffect statusEffect;
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";

    public ApplyStatus() {
        effectStepName = "ApplyStatus";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> combatInstances = document.map.GetList<CombatInstance>(inputKey);
        if (combatInstances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }

        // Setup the scale
        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }

        foreach (CombatInstance combatInstance in combatInstances) {
            combatInstance.ApplyStatusEffects(statusEffect, finalScale);
        }
        yield return null;
    }

    public TooltipViewModel GetTooltip() {
        if(KeywordTooltipProvider.Instance.HasTooltip(statusEffect))
        {
            return KeywordTooltipProvider.Instance.GetTooltip(statusEffect);
        }
        return new TooltipViewModel(empty: true);
    }
}