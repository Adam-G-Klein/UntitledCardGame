using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bellows: EffectProcedure {
    public int cardsToDraw = 4;
    public int cardsToExhaust = 2;
    private List<EntityType> validTargets = new List<EntityType>(){EntityType.Companion, EntityType.Minion};

    public Bellows() {
        procedureClass = "Bellows";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        yield return base.prepare(context);
        context.cardCastManager.requestTarget(validTargets, this);
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        CombatEntityWithDeckInstance target = (CombatEntityWithDeckInstance)currentTargets[0];
        List<Card> cards = target.inCombatDeck.dealCardsFromDeck(cardsToDraw);
        context.cardCastManager.raiseCardSelectionRequest(new CardSelectionRequestEventInfo(cards, CardSelectionAction.EXHAUST, CardSelectionAction.ADD_TO_HAND));
        yield return null;
    }

}