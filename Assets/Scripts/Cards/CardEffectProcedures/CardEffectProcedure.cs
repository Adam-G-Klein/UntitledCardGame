using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffectTargetType {
    // Select some cards from the draw pile, reshuffling to replenish if possible and necessary
    CompanionDeckWithReshuffle,
    // Select a few cards from the hand
    Hand,
    // Effect targets entire hand
    EntireHand,
    // effect targets entire deck of targeted companion
    EntireDeck
    //TODO:
    //CompanionDeckWithoutReshuffle,
}

[System.Serializable]
public class CardEffectProcedure: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public CardEffectTargetType targetType;
    public CardEffect selectedEffect = CardEffect.None;
    public CardEffect unselectedEffect = CardEffect.None;
    [Tooltip("Only for companion targets")]
    public int cardsToDraw = 4;
    public int minSelections = 0;
    public int maxSelections = int.MaxValue;
    private List<Card> cardsToSelectFrom;

    public CardEffectProcedure() {
        procedureClass = "CardEffectProcedure";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        List<PlayableCard> cardsInHand = context.playerHand.cardsInHand;
        // damn this code is messy. Will clean it up if I need to revisit it
        switch(targetType) {
            case CardEffectTargetType.CompanionDeckWithReshuffle:
                context.cardCastManager.requestTarget(new List<EntityType>(){EntityType.Companion}, this);
                yield return new WaitUntil(() => currentTargets.Count > 0);
                CombatEntityWithDeckInstance target = (CombatEntityWithDeckInstance) currentTargets[0];
                cardsToSelectFrom = target.inCombatDeck.dealCardsFromDeck(cardsToDraw, true);
                yield return context.cardCastManager.raiseCardSelectionRequest(new CardSelectionRequestEventInfo(cardsToSelectFrom, selectedEffect, unselectedEffect, minSelections, maxSelections)); 
                break;
            case CardEffectTargetType.EntireDeck:
                context.cardCastManager.requestTarget(new List<EntityType>(){EntityType.Companion}, this);
                yield return new WaitUntil(() => currentTargets.Count > 0);
                target = (CombatEntityWithDeckInstance) currentTargets[0];
                cardsToSelectFrom.AddRange(target.inCombatDeck.drawPile);
                cardsToSelectFrom.AddRange(target.inCombatDeck.discardPile);    
                foreach(Card card in cardsToSelectFrom) {
                    yield return context.cardCastManager.raiseCardEffect(new CardEffectEventInfo(
                        new Dictionary<CardEffect, int>(){
                            {selectedEffect, 1}
                        },
                        null,
                        new List<Card>(){card}
                        ));
                }
                break;
            case CardEffectTargetType.Hand:
                cardsToSelectFrom.AddRange(cardsInHand.ConvertAll(c => c.outOfCombatCard));
                cardsToSelectFrom.Remove(context.cardCasting);
                // need to do this here because it's technically target selection,
                // feels weird to have the window pop up after you've selected targets for combat effect procedures
                // that are on the same card
                yield return context.cardCastManager.raiseCardSelectionRequest(new CardSelectionRequestEventInfo(cardsToSelectFrom, selectedEffect, unselectedEffect, minSelections, maxSelections)); 
                break;
            case CardEffectTargetType.EntireHand:
                cardsToSelectFrom.AddRange(cardsInHand.ConvertAll(c => c.outOfCombatCard));
                cardsToSelectFrom.Remove(context.cardCasting);
                foreach(Card card in cardsToSelectFrom) {
                    yield return context.cardCastManager.raiseCardEffect(new CardEffectEventInfo(
                        new Dictionary<CardEffect, int>(){
                            {selectedEffect, 1}
                        },
                        null,
                        new List<Card>(){card}
                        ));
                }
                break;

        }
        // passes back to the cardCaster, where it will call invoke
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        yield return null;
    }

    public bool effectNeedsTargets(CombatEffect effect) {
        return targetType != CardEffectTargetType.Hand;
    }

    public override void resetCastingState() {
        base.resetCastingState();
        cardsToSelectFrom.Clear();
    }

}