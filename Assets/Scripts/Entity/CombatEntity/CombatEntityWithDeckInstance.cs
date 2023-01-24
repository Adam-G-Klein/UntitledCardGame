using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Just in the hierarchy so that we don't duplicate this code for minions
// and companions 
public abstract class CombatEntityWithDeckInstance : CombatEntityInstance
{
    //  Don't have great justification for this being public,
    // other than the fact that a getter/setter inline would be recursive
    // Also it's an interface so it won't serialize in the editor anyways
    public CombatEntityWithDeck deckEntity; 
    [SerializeField]
    private CardsDealtEvent cardsDealtEvent;
    [SerializeField]
    private GameplayConstants constants;
    public InCombatDeck inCombatDeck;
    private TurnManager turnManager;

    protected override void Start()
    {
        base.Start();
        inCombatDeck = new InCombatDeck(deckEntity.getDeck());
        GameObject turnManagerObject = GameObject.Find("TurnManager");
        if(turnManagerObject != null)  turnManager = turnManagerObject.GetComponent<TurnManager>();
        else Debug.LogError("No TurnManager found in scene, companions won't deal cards");
        turnManager.addTurnPhaseTrigger(new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, dealStartTurnCards()));
        // Tried doing this in Awake, but it looks like the fields of companion
        // hadn't been initialized by then
    }

    public void dealCards(int numCards){
        List<Card> cards = inCombatDeck.dealCardsFromDeck(numCards);
        StartCoroutine(cardsDealtEvent.RaiseAtEndOfFrameCoroutine(new CardsDealtEventInfo(cards, inCombatDeck, stats)));
    }

    public IEnumerable dealStartTurnCards() {
        dealCards(constants.START_TURN_DRAW_PER_COMPANION);
        yield return null;
    }
    
}

