using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectForStatusCards: EffectProcedure {
    public DisplayedCombatEffect combatEffect;
    public int effectScale = 5;
    public int statusCardsToShuffle = 1;
    public CardType statusCardType;
    private List<EntityType> validTargets = new List<EntityType>(){EntityType.Companion, EntityType.Minion};

    public EffectForStatusCards() {
        procedureClass = "EffectForStatusCards";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        yield return base.prepare(context);
        context.cardCastManager.requestTarget(validTargets, this);
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        // trusting targetting
        context.cardCastManager.raiseCombatEffect(CombatEffectProcedure.displayedToCombatEffect[combatEffect], effectScale, currentTargets, context.cardCaster);
        CombatEntityWithDeckInstance target = (CombatEntityWithDeckInstance) currentTargets[0];
        // TODO: make shuffling in a status card a combat effect
        target.inCombatDeck.addToDiscard(new Card(statusCardType));
        yield return null;
    }

}