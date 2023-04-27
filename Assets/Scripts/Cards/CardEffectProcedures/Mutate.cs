using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mutate: EffectProcedure {
    /*
        Shuffle X copies of a card in your hand into each minion token’s deck.
    */
    public int numCopies = 1;
    public bool targetAllValidTargets = false;
    public List<EntityType> validTargets = new List<EntityType>() {
        EntityType.Minion
    };
    private List<Card> cardsToShuffleIn = new List<Card>();

    public Mutate() {
        procedureClass = "Mutate";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        // get card target
        TargettingManager.Instance.requestTargets(
            this,
            context.origin,
            new List<EntityType>() { EntityType.PlayableCard });
        yield return new WaitUntil(() => currentTargets.Count > 0);
        Card cardToShuffleIn = PlayerHand.Instance
            .getCardById(currentTargets[0].id).card;
        // clear to prep to use the list for the targets of the shuffling in
        currentTargets.Clear(); 
        for(int i = 0; i < numCopies; i++) {
            cardsToShuffleIn.Add(new Card(cardToShuffleIn));
        }
        if(targetAllValidTargets) {
            currentTargets.AddRange(
                TargettingManager.Instance.getAllValidTargets(validTargets));
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
        cardsToShuffleIn.Clear();
    }

}