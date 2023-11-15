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
        List<CombatInstance> instances = document.map.GetList<CombatInstance>(inputKey);
        if (instances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }

        CombatInstance origin = null;
        // Determine whether origin of damage is from a card, companion ability, or enemy attack
        // and get that origin
        if (document.map.ContainsValueWithKey<PlayableCard>(EffectDocument.ORIGIN)) {
            PlayableCard card = document.map.GetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
            origin = card.deckFrom.combatInstance;
        } else if (document.map.ContainsValueWithKey<CompanionInstance>(EffectDocument.ORIGIN)) {
            origin = document.map.GetItem<CompanionInstance>(EffectDocument.ORIGIN, 0).combatInstance;
        } else if (document.map.ContainsValueWithKey<EnemyInstance>(EffectDocument.ORIGIN)) {
            origin = document.map.GetItem<EnemyInstance>(EffectDocument.ORIGIN, 0).combatInstance;
        } else {
            EffectError("No origin set in EffectDocument to pull stats from");
            yield return null;
        }

        int baseScale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            baseScale = document.intMap[inputScaleKey];
        } else {
            baseScale = scale;
        }

        int finalScale = UpdateScaleForEffect(baseScale, origin);

        foreach (CombatInstance instance in instances) {
            instance.ApplyNonStatusCombatEffect(combatEffect, finalScale, origin);
        }

        yield return null;
    }

    private int UpdateScaleForEffect(int baseScale, CombatInstance origin) {
        int newScale = baseScale;
        switch (combatEffect) {
            case CombatEffect.Damage:
                newScale = (baseScale + origin.GetCurrentDamage()) 
                    * origin.statusEffects[StatusEffect.DamageMultiply];
            break;
        }

        return newScale;
    }
}
