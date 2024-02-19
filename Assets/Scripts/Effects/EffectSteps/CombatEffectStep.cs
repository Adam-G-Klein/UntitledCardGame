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
    [SerializeField]
    private int multiplicity = 1;
    [SerializeField]
    private bool getMultiplicityFromKey = false;
    [SerializeField]
    private string inputMultiplicityKey = "";

    public CombatEffectStep() {
        effectStepName = "CombatEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> instances = document.map.GetList<CombatInstance>(inputKey);
        if (instances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }

        CombatInstance originCombatInstance = null;
        PlayableCard originCard = null;
        // Determine whether origin of damage is from a card, companion ability, or enemy attack
        // and get that origin
        if (document.map.ContainsValueWithKey<PlayableCard>(EffectDocument.ORIGIN)) {
            originCard = document.map.GetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
            originCombatInstance = originCard.deckFrom.combatInstance;
        } else if (document.map.ContainsValueWithKey<CompanionInstance>(EffectDocument.ORIGIN)) {
            originCombatInstance = document.map.GetItem<CompanionInstance>(EffectDocument.ORIGIN, 0).combatInstance;
        } else if (document.map.ContainsValueWithKey<EnemyInstance>(EffectDocument.ORIGIN)) {
            originCombatInstance = document.map.GetItem<EnemyInstance>(EffectDocument.ORIGIN, 0).combatInstance;
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

        int baseMultiplicity;
        if (getMultiplicityFromKey && document.intMap.ContainsKey(inputMultiplicityKey)) {
            baseMultiplicity = document.intMap[inputMultiplicityKey];
        } else {
            baseMultiplicity = multiplicity;
        }

        int finalScale = UpdateScaleForEffect(baseScale, originCombatInstance, originCard);

        foreach (CombatInstance instance in instances) {
            for (int i = 0; i < baseMultiplicity; i++) {
                if (instance != null) { // This protects against the case in which the enemy is destroyed before all hits of the damage are completed
                    instance.ApplyNonStatusCombatEffect(combatEffect, finalScale, originCombatInstance);
                }
            }
        }

        yield return null;
    }

    private int UpdateScaleForEffect(
            int baseScale,
            CombatInstance origin,
            PlayableCard card = null) {
        int newScale = baseScale;
        switch (combatEffect) {
            case CombatEffect.Damage:
                newScale = baseScale + origin.GetCurrentDamage();
                if (card != null) {
                    newScale = card.card.UpdateScaleForCardModifications(newScale);
                }
            break;
        }

        return newScale;
    }
}
