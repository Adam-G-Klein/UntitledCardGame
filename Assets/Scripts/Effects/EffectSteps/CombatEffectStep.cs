using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that deals with common combat effects (non status effects) such as
    dealing damage, healing, etc...

    Input: The entities that are the targets of the effect
    Output: NA
    Parameters:
        - CombatEffect: The effect to use in the step
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class CombatEffectStep : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private CombatEffect combatEffect;
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";

    public CombatEffectStep() {
        effectStepName = "CombatEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatEntityInstance> entities = document.getCombatEntityInstances(inputKey);
        if (entities.Count == 0) {
            Debug.LogError("Damage Effect: No input targets present for key " + inputKey);
            yield return null;
        }

        CombatEntityInstance origin = null;
        // Determine whether origin of damage is from a card or a companion ability
        // and get either the entity that delt the card or the companion who's ability
        // is going off
        if (document.playableCardMap.containsValueWithKey(EffectDocument.ORIGIN)) {
            PlayableCard card = document.playableCardMap.getItem(EffectDocument.ORIGIN, 0);
            origin = card.entityFrom;
        } else if (document.companionMap.containsValueWithKey(EffectDocument.ORIGIN)) {
            origin = document.companionMap.getItem(EffectDocument.ORIGIN, 0);
        } else {
            Debug.LogError("Damage Effect: No origin set in EffectDocument to pull stats from");
            yield return null;
        }

        int finalScale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        } else {
            finalScale = (scale + origin.stats.currentAttackDamage) 
                    * origin.stats.statusEffects[StatusEffect.DamageMultiply];
        }

        foreach (CombatEntityInstance entity in entities) {
            entity.applyNonStatusCombatEffect(combatEffect, finalScale, origin);
        }

        yield return null;
    }
}
