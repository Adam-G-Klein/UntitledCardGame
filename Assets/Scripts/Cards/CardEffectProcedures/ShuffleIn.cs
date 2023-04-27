using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShuffleIn: EffectProcedure {
    public CardType cardToShuffleIn;
    public int numCards = 1;
    public bool targetCaster = false;
    public List<EntityType> validTargets = new List<EntityType>() {EntityType.Companion, EntityType.Minion};
    private List<Card> cardsToShuffleIn = new List<Card>();

    public ShuffleIn() {
        procedureClass = "ShuffleIn";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        for(int i = 0; i < numCards; i++) {
            cardsToShuffleIn.Add(new Card(cardToShuffleIn));
        }
        if(targetCaster) {
            currentTargets.Add(context.cardCaster);
        } else {
            TargettingManager.Instance.requestTargets(this, context.origin, validTargets);
        }
        yield return new WaitUntil(() => currentTargets.Count > 0);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        foreach(CombatEntityWithDeckInstance target in currentTargets) {
            // will need to be a coroutine we yield on when we want to animate this
            target.inCombatDeck.shuffleIntoDraw(cardsToShuffleIn);
        }
        yield return null;
    }

    public override void resetCastingState(){
        currentTargets.Clear();
    }

}