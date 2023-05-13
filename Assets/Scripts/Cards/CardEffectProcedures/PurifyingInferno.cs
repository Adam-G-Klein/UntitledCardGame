using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PurifyingInferno: EffectProcedure {
    /*
        Choose a card in a minion’s deck.
        Add it to your hand. Exhaust the rest of the deck
    */
    public int cardsToChoose = 1;
    private List<EntityType> validTargets = new List<EntityType>() {
        EntityType.Companion,
        EntityType.Minion
    };

    public PurifyingInferno() {
        procedureClass = "PurifyingInferno";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        yield return base.prepare(context);
        TargettingManager.Instance.requestTargets(this, context.origin, validTargets);
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        CombatEntityWithDeckInstance target = CombatEntityManager.Instance
            .getEntityWithDeckById(currentTargets[0].id);
        List<Card> cards = target.inCombatDeck.getAllCards();
        TargettingManager.Instance.raiseCardSelectionRequest(
            new CardSelectionRequestEventInfo(
                cards,
                CardEffectType.AddToHand,
                CardEffectType.Exhaust,
                cardsToChoose,
                cardsToChoose));
        yield return null;
    }

}