using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffectProcedure: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public CardEffect effectName;
    public int baseScale = 0;
    public bool targetAllValidTargets = false;
    public bool requiresUniqueTarget = false;
    private List<EntityType> validTargets = new List<EntityType>(){EntityType.PlayableCard};

    public CardEffectProcedure() {
        procedureClass = "CardEffectProcedure";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        if(targetAllValidTargets) {
            currentTargets.AddRange(
                TargettingManager.Instance.getAllValidTargets(validTargets));
        } else {
            TargettingManager.Instance.requestTargets(
                this,
                context.origin,
                validTargets,
                requiresUniqueTarget ? context.alreadyTargetted : null);
        }
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
        // passes back to the cardCaster, where it will call invoke
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        TargettingManager.Instance.raiseCardEffect(
            new CardEffectEventInfo(
                new Dictionary<CardEffect, int> {
                    {effectName, baseScale}
                },
                currentTargets
            )
        );
        yield return null;
    }

    public bool effectNeedsTargets(CombatEffect effect) {
        // everything needs targets right now,
        // even if it's just the caster or the full set 
        // of valid targets from the cardCastManager
        return true;
    }

}