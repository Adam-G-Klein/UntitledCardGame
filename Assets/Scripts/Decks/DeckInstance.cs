using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckInstance : MonoBehaviour
{
    public Deck sourceDeck;
    public List<Card> drawPile;
    public List<Card> discardPile;
    public List<Card> inHand;
    public CombatInstance combatInstance;

    private TurnPhaseTrigger drawCardsTurnPhaseTrigger;

    private float nextMinionSpawnTheta = Mathf.PI/2f;
    private float minionSpawnRadius = 3f;

    public void Start() {
        SetupPiles(sourceDeck);
        RegisterDrawTrigger();
        combatInstance.onDeathHandler += OnDeath;
    }

    private void RegisterDrawTrigger() {
        drawCardsTurnPhaseTrigger = new TurnPhaseTrigger(
            TurnPhase.START_PLAYER_TURN, dealStartPlayerTurnCards());
        TurnManager.Instance.addTurnPhaseTrigger(drawCardsTurnPhaseTrigger);
    }

    private void UnregisterDrawTrigger() {
        TurnManager.Instance.removeTurnPhaseTrigger(drawCardsTurnPhaseTrigger);
    }

    private IEnumerator OnDeath(CombatInstance killer) {
        UnregisterDrawTrigger();
        yield return null;
    }

    public IEnumerable dealStartPlayerTurnCards() {
        DealCardsToPlayerHand(sourceDeck.cardsDealtPerTurn);
        yield return null;
    }

    private void SetupPiles(Deck sourceDeck) {
        this.sourceDeck = sourceDeck;
        this.drawPile = new List<Card>();
        foreach(Card card in sourceDeck.cards) {
            this.drawPile.Add(new Card(card));
        }
        this.discardPile = new List<Card>();
        this.inHand = new List<Card>();
    }

    public List<PlayableCard> DealCardsToPlayerHand(int numCards) {
        List<Card> cards = DealCardsFromDeck(numCards);
        return PlayerHand.Instance.DealCards(cards, this);
    }

    public List<Card> DealCardsFromDeck(int numCards, bool withReplacement = false){
        List<Card> returnList = new List<Card>();
        ShuffleDeck();
        for(int i = 0; i < numCards; i++){
            DealCardFromDeckToList(returnList, withReplacement);
        }
        inHand.AddRange(returnList);
        return returnList;
    }

    private void DealCardFromDeckToList(List<Card> toList, bool withReplacement = false) {
        // Check to see if the draw pile is empty
        if (drawPile.Count == 0) {
            // If draw pile is empty, the options are shuffle in the discard pile then draw,
            // or we just get no draw since both are empty :(
            if (discardPile.Count == 0) {
                // big loser moment
                return;
            }
            ShuffleDiscardIntoDraw();
        }
        Card card = drawPile[0];
        if (!withReplacement)
            drawPile.Remove(card);
        if (!toList.Contains(card)) {
            toList.Add(card);
        }
    }

    private void ShuffleDeck() {
         System.Random _random = new System.Random();
         Card temp;
 
         int n = drawPile.Count;
         for (int i = 0; i < n; i++)
         {
             // NextDouble returns a random number between 0 and 1
             int r = i + (int)(_random.NextDouble() * (n - i));
             temp = drawPile[r];
             drawPile[r] = drawPile[i];
             drawPile[i] = temp;
         }
     }

    private void ShuffleDiscardIntoDraw(){
        drawPile.AddRange(discardPile);
        drawPile.Shuffle();
        discardPile.Clear();
    }

    public void DiscardCards(List<Card> cards){
        inHand.RemoveAll(c => cards.Contains(c));
        discardPile.AddRange(cards);
    }

    public void ShuffleIntoDraw(List<Card> cards){
        Debug.Log("Shuffling " + cards.Count + " cards into draw pile");
        drawPile.AddRange(cards);
    }

    public bool ContainsCardById(string id){
        return drawPile.Exists(c => c.id == id) || discardPile.Exists(c => c.id == id);
    }

    public Card GetCardById(string id){
        Card card = drawPile.Find(c => c.id == id);
        if(card == null){
            card = discardPile.Find(c => c.id == id);
        }
        return card;
    }

    public void RemoveFromDraw(Card card){
        drawPile.Remove(card);
    }

    public void ExhaustCard(Card card){
        if(drawPile.Contains(card)){
            Debug.Log("Exhausting card " + card.id + " from draw pile");
            drawPile.Remove(card);
        }
        else if(discardPile.Contains(card)){
            Debug.Log("Exhausting card " + card.id + " from discard pile");
            discardPile.Remove(card);
        } else if (inHand.Contains(card)) {
            Debug.Log("Exhausting card " + card.id + " from hand");
            inHand.Remove(card);
        }
    }

    public void DiscardCard(Card card){
        if(drawPile.Contains(card)){
            Debug.Log("Discarding card " + card.id + " from draw pile");
            drawPile.Remove(card);
            discardPile.Add(card);
        } else if (inHand.Contains(card)) {
            Debug.Log("Discarding card " + card.id + " from hand");
            inHand.Remove(card);
            discardPile.Add(card);
        }
    }

    public void PurgeCard(Card card){
        Debug.Log("Purging card " + card.id + " from deck");
        if(drawPile.Contains(card)){
            drawPile.Remove(card);
        }
        else if(discardPile.Contains(card)){
            discardPile.Remove(card);
        } else if (inHand.Contains(card)) {
            inHand.Remove(card);
        }
        sourceDeck.PurgeCard(card.id);
    }

    public void AddToDiscard(Card card){
        discardPile.Add(card);
        if(inHand.Contains(card)) {
            inHand.Remove(card);
        }
    }

    public List<Card> GetAllCards(){
        List<Card> cards = new List<Card>();
        cards.AddRange(drawPile);
        cards.AddRange(discardPile);
        cards.AddRange(inHand);
        return cards;
    }

    public bool Contains(Card card){
        return drawPile.Contains(card) || discardPile.Contains(card);
    }

    public Vector2 getNextMinionSpawnPosition() {
        Vector2 center = transform.position;
        // from copilot and https://answers.unity.com/questions/1545128/how-can-i-get-a-point-position-in-circle-line.html
        Vector2 spawnLoc = new Vector2(
            center.x + minionSpawnRadius * Mathf.Cos(nextMinionSpawnTheta),
            center.y + minionSpawnRadius * Mathf.Sin(nextMinionSpawnTheta)
        );
        nextMinionSpawnTheta += 2 * Mathf.PI / GameplayConstantsSingleton.Instance.gameplayConstants.MAX_MINIONS_PER_COMPANION;
        return spawnLoc;
    }
}