using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class InCombatDeck
{
    public Deck sourceDeck;
    public List<Card> drawPile;
    public List<Card> discardPile;

    public InCombatDeck(Deck startingDeck)
    {
        this.sourceDeck = startingDeck;
        this.drawPile = new List<Card>();
        foreach(Card card in startingDeck.cards) {
            this.drawPile.Add(new Card(card));
        }
        this.discardPile = new List<Card>();
    }
    public InCombatDeck(List<Card> cards)
    {
        this.sourceDeck = new Deck(cards);
        this.drawPile = new List<Card>();
        this.drawPile.AddRange(cards);
        this.discardPile = new List<Card>();
    }


    // have tried breaking this function up a couple times, 
    public List<Card> dealCardsFromDeck(int numCards, bool withReplacement = false){
        List<Card> returnList = new List<Card>();
        for(int i = 0; i < numCards; i++){
            dealCardFromDeckToList(returnList, withReplacement, i);
        }
        return returnList;
    }

    // pass the list reference around for the case where the deck is empty
    // so we don't have to do any weird logic around checking null on the return value
    // *Note* this function will shuffle the deck to get a card if the draw pile is empty
    // TODO: a scrying function that doesn't shuffle if we need it
    private void dealCardFromDeckToList(List<Card> toList, bool withReplacement = false, int cardinality = 0) {
        int totalCards = drawPile.Count + discardPile.Count;
        if(cardinality >= totalCards)
            return;
        // cardinality = 0 and drawpile = 0 is a case where we need to shuffle the discard pile into the draw pile
        if (drawPile.Count <= cardinality) {
            if (discardPile.Count == 0) {
                // No cards left in deck or discard pile
                // Don't add anything to the list
                return;
            }
            // Shuffle discard pile into draw pile
            shuffleDiscardIntoDraw();
        }
        Card card = drawPile[cardinality];
        if (!withReplacement)
            drawPile.Remove(card);
        if(!toList.Contains(card)) {
            toList.Add(card);
        }
    }

    private void shuffleDiscardIntoDraw(){
        drawPile.AddRange(discardPile);
        drawPile.Shuffle();
        discardPile.Clear();
    }

    public void discardCards(List<Card> cards){
        discardPile.AddRange(cards);
    }

    public void shuffleIntoDraw(List<Card> cards){
        Debug.Log("Shuffling " + cards.Count + " cards into draw pile");
        drawPile.AddRange(cards);
    }

    public bool containsCardById(string id){
        return drawPile.Exists(c => c.id == id) || discardPile.Exists(c => c.id == id);
    }

    public Card getCardById(string id){
        Card card = drawPile.Find(c => c.id == id);
        if(card == null){
            card = discardPile.Find(c => c.id == id);
        }
        return card;
    }

    public void removeFromDraw(Card card){
        drawPile.Remove(card);
    }

    public void exhaustCard(Card card){
        if(drawPile.Contains(card)){
            Debug.Log("Exhausting card " + card.id + " from draw pile");
            drawPile.Remove(card);
        }
        else if(discardPile.Contains(card)){
            Debug.Log("Exhausting card " + card.id + " from discard pile");
            discardPile.Remove(card);
        }
    }

    public void discardCard(Card card){
        if(drawPile.Contains(card)){
            Debug.Log("Discarding card " + card.id + " from draw pile");
            drawPile.Remove(card);
            discardPile.Add(card);
        }
    }

    public void purgeCard(Card card){
        if(drawPile.Contains(card)){
            drawPile.Remove(card);
        }
        else if(discardPile.Contains(card)){
            discardPile.Remove(card);
        }
        sourceDeck.cards.Remove(card);
    }

    public void addToDiscard(Card card){
        discardPile.Add(card);
    }

    public List<Card> getAllCards(){
        List<Card> cards = new List<Card>();
        cards.AddRange(drawPile);
        cards.AddRange(discardPile);
        return cards;
    }

    public bool Contains(Card card){
        return drawPile.Contains(card) || discardPile.Contains(card);
    }
}
