using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Brainstorm: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public const string description = "Deal damage equal to the number of cards in your hand to a target.";
    public int baseScale = 0;
    public List<EntityType> validTargets = new List<EntityType>() {EntityType.Enemy};

    public Brainstorm() {
        procedureClass = "Brainstorm";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        context.cardCastManager.requestTarget(validTargets, this);
        yield return new WaitUntil(() => currentTargets.Count > 0);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        // Subtract 1 because we don't want to count the card we're playing
        int damage = (PlayerHand.Instance.cardsInHand.Count - 1);
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