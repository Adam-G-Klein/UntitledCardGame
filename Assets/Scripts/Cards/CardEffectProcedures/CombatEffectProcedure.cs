using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatEffectProcedure: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public string procedureClass;
    public CombatEffect effectName;
    public int baseScale = 0;
    public bool targetCaster = false;
    public bool targetAllValidTargets = false;
    public List<EntityType> validTargets;

    public CombatEffectProcedure() {
        procedureClass = "CombatEffectProcedure";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        if(!effectNeedsTargets(effectName)) {
            yield break;
        }
        if(targetAllValidTargets) {
            currentTargets.AddRange(context.cardCastManager.getAllValidTargets(validTargets));
        } else if(targetCaster) {
            currentTargets.Add(context.cardCaster);
        }
        else {
            context.cardCastManager.requestTarget(validTargets, this);
        }
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
        // passes back to the cardCaster, where it will call invoke
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        context.cardCastManager.raiseCombatEffect(
            new CombatEffectEventInfo(
                new Dictionary<CombatEffect, int> {
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