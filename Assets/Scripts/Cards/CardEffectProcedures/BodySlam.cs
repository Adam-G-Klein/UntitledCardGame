using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodySlam: EffectProcedure {
    public const string description = "Deal damage equal to X% of this companion's max HP to a target.";
    public int maxHPPercentage = 0;
    public List<EntityType> validTargets = new List<EntityType>() {EntityType.Enemy};

    public BodySlam() {
        procedureClass = "BodySlam";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        context.cardCastManager.requestTarget(validTargets, this);
        yield return new WaitUntil(() => currentTargets.Count > 0);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        int damage = context.casterStats.maxHealth * maxHPPercentage / 100;
        context.cardCastManager.raiseCombatEffect(
            new CombatEffectEventInfo(
                new Dictionary<CombatEffect, int> {
                    {CombatEffect.Damage, context.casterStats.getDamage(damage)}
                },
                currentTargets,
                context.cardCaster
            )
        );
        yield return null;
    }

    public override void resetCastingState(){
        currentTargets.Clear();
    }

}