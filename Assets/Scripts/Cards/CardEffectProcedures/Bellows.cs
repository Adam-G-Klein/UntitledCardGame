using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bellows: EffectProcedure {
    public int cardsToDraw = 4;
    public int cardsToExhaust = 1;
    private List<EntityType> validTargets = new List<EntityType>(){EntityType.Companion, EntityType.Minion};

    public Bellows() {
        procedureClass = "Bellows";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        yield return base.prepare(context);
        context.cardCastManager.requestCardTargets(CardEffectTargetType.FromDeckWithReshuffle, CardEffect.Exhaust, cardsToDraw, cardsToExhaust, cardsToExhaust, this);
        yield return new WaitUntil(() => this.currentUnselectedCardTargets.Count > 0 || this.currentSelectedCardTargets.Count > 0);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        context.cardCastManager.raiseCardEffects(CardEffect.Exhaust, CardEffect.AddToHand, currentSelectedCardTargets, currentUnselectedCardTargets);
        yield return null;
    }

}