using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectIncreasesOnPlay: EffectProcedure {
    public DisplayedCombatEffect combatEffect;
    public int baseScale = 1;
    public int increaseOnPlay = 1;
    public List<EntityType> validTargets = new List<EntityType>() {EntityType.Enemy};

    public EffectIncreasesOnPlay() {
        procedureClass = "EffectIncreasesOnPlay";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        TargettingManager.Instance.requestTargets(this, context.origin, validTargets);
        yield return new WaitUntil(() => currentTargets.Count > 0);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        CombatEffect internalEffect = CombatEffectProcedure.displayedToCombatEffect[combatEffect];
        // Instantiates a new CombatEffectEventInfo, so we don't need to worry about messing up the reference
        Debug.Log("effectBuffs: " + context.castingCard.getEffectBuff(internalEffect));
        int scale = context.castingCard.getEffectBuff(internalEffect) + baseScale;
        CombatEntityManager.Instance.handleCombatEffect(
            new CombatEffectEventInfo(
                new Dictionary<CombatEffect, int> {
                    {
                        internalEffect,
                        context.cardCaster.stats.getEffectScale(internalEffect, scale)
                    }
                },
                currentTargets,
                context.cardCaster
            )
        );
        context.castingCard.buffEffect(internalEffect, increaseOnPlay);
        yield return null;
    }


}