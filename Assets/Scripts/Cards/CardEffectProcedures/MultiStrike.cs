using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MultiStrike: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public const string description = "Perform multiple separate strikes";
    public int numStrikes;
    public int baseScale = 0;
    public float strikeDelay = 0.2f;
    public List<EntityType> validTargets = new List<EntityType>() {
        EntityType.Enemy
    };

    public MultiStrike() {
        procedureClass = "MultiStrike";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        TargettingManager.Instance.requestTargets(this, context.origin, validTargets);
        yield return new WaitUntil(() => currentTargets.Count > 0);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        for(int i = 0; i < numStrikes; i++) {
            CombatEntityManager.Instance.handleCombatEffect(
                new CombatEffectEventInfo(
                    new Dictionary<CombatEffect, int> {
                        {CombatEffect.Damage, context.casterStats.getDamage(baseScale)}
                    },
                    currentTargets,
                    context.cardCaster
                )
            );
            yield return new WaitForSeconds(strikeDelay);
        }
    }


    public override void resetCastingState(){
        currentTargets.Clear();
    }


}