using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Just in the hierarchy so that we don't duplicate this code for minions
// and companions 
[RequireComponent(typeof(CardEffectEventListener))]
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

    [SerializeField]
    private float nextMinionSpawnTheta = Mathf.PI/2f;
    [SerializeField]
    private float minionSpawnRadius = 3f;
    private TurnPhaseTrigger startTurnTrigger;

    protected override void Start()
    {
        base.Start();
        inCombatDeck = new InCombatDeck(deckEntity.getDeck());
        GameObject turnManagerObject = GameObject.Find("TurnManager");
        startTurnTrigger = new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, dealStartTurnCards());
        registerTurnPhaseTriggerEvent.Raise(new TurnPhaseTriggerEventInfo(startTurnTrigger));
        // Tried doing this in Awake, but it looks like the fields of companion
        // hadn't been initialized by then
    }

    public List<PlayableCard> dealCards(int numCards){
        List<Card> cards = inCombatDeck.dealCardsFromDeck(numCards);
        // StartCoroutine(cardsDealtEvent.RaiseAtEndOfFrameCoroutine(new CardsDealtEventInfo(cards, this)));
        return PlayerHand.Instance.dealCards(cards, this);
    }
    protected override void onDraw(int scale)
    {
        dealCards(scale);
    }

    // Here so that we can target cards in decks with effect events
    public void onCardEffectEvent(CardEffectEventInfo info)
    {
        foreach(Card card in info.cards) {
            if(inCombatDeck.Contains(card)) {
                applyCardEffects(info.cardEffects, card);
            }
        }
    }

    public void applyCardEffects(Dictionary<CardEffectType, int> effects, Card card)
    {
        foreach(KeyValuePair<CardEffectType, int> effect in effects)
        {
            applyCardEffect(effect.Key, effect.Value, card);
        }
    }

    public void applyCardEffect(CardEffectType effect, int scale, Card card)
    {
        switch(effect)
        {
            case CardEffectType.AddToHand:
                // if we're here, the card is in the deck
                // so for now we won't differentiate between this and 
                // just drawing it (even though MTG would not trigger draw card effects)
                Debug.Log("Adding card to hand " + card.name + " " + card.id);
                inCombatDeck.removeFromDraw(card);
                StartCoroutine(cardsDealtEvent.RaiseAtEndOfFrameCoroutine(
                        new CardsDealtEventInfo(new List<Card>{card}, this)));
                break;
            case CardEffectType.Exhaust:
                inCombatDeck.exhaustCard(card);
                break;
            case CardEffectType.Discard:
                inCombatDeck.discardCard(card);
                break;
            case CardEffectType.Purge:
                inCombatDeck.purgeCard(card);
                break;
            default:
                Debug.LogError("Unrecognized card effect " + effect);
                break;
        }
    }

    protected override IEnumerator onDeath(CombatEntityInstance killer) {
        yield return StartCoroutine(removeTurnPhaseTriggerEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseTriggerEventInfo(startTurnTrigger)));
        yield return base.onDeath(killer);
    }

    public IEnumerable dealStartTurnCards() {
        dealCards(constants.START_TURN_DRAW_PER_COMPANION);
        yield return null;
    }

    // any entity with a deck could *theoretically* summon minions
    // maybe we raise this into combatentityinstance at some point
    public Vector2 getNextMinionSpawnPosition() {
        Vector2 center = transform.position;
        // from copilot and https://answers.unity.com/questions/1545128/how-can-i-get-a-point-position-in-circle-line.html
        Vector2 spawnLoc = new Vector2(
            center.x + minionSpawnRadius * Mathf.Cos(nextMinionSpawnTheta),
            center.y + minionSpawnRadius * Mathf.Sin(nextMinionSpawnTheta)
        );
        nextMinionSpawnTheta += 2 * Mathf.PI / constants.MAX_MINIONS_PER_COMPANION;
        return spawnLoc;
    }

}

