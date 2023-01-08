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
        this.drawPile.AddRange(startingDeck.cards);
        this.discardPile = new List<Card>();
    }
    public InCombatDeck(List<Card> cards)
    {
        this.sourceDeck = new Deck(cards);
        this.drawPile = new List<Card>();
        this.drawPile.AddRange(cards);
        this.discardPile = new List<Card>();
    }


    public List<Card> dealCardsFromDeck(int numCards){
        List<Card> returnList = new List<Card>();
        Card card;
        for(int i = 0; i < numCards; i++){
            if(drawPile.Count == 0){
                if (discardPile.Count == 0) {
                    // No cards left in deck or discard pile
                    return returnList;
                }
                // Shuffle discard pile into draw pile
                drawPile.AddRange(discardPile);
                discardPile.Clear();
            }
            card = drawPile[Random.Range(0, drawPile.Count)];
            returnList.Add(card);
            drawPile.Remove(card);
        }
        return returnList;
    }

    public void discardCards(List<Card> cards){
        discardPile.AddRange(cards);
    }
}
