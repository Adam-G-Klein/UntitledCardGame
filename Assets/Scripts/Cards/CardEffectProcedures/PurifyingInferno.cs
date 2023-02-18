using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PurifyingInferno: EffectProcedure {
    public int cardsToChoose = 1;
    private List<EntityType> validTargets = new List<EntityType>(){EntityType.Companion, EntityType.Minion};

    public PurifyingInferno() {
        procedureClass = "PurifyingInferno";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        yield return base.prepare(context);
        context.cardCastManager.requestTarget(validTargets, this);
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        CombatEntityWithDeckInstance target = (CombatEntityWithDeckInstance) currentTargets[0];
        List<Card> cards = target.inCombatDeck.getAllCards();
        context.cardCastManager.raiseCardSelectionRequest(new CardSelectionRequestEventInfo(cards, CardEffect.AddToHand, CardEffect.Exhaust, cardsToChoose, cardsToChoose));
        yield return null;
    }

}