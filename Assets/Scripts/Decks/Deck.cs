using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Deck
{
    public StartingDeck startingDeck;
    [SerializeField]
    public List<Card> cards = new List<Card>();
    public int cardsDealtPerTurn = 1;

    public Deck(StartingDeck startingDeck, int cardsDealtPerTurn = 1)
    {
        this.startingDeck = startingDeck;
        this.cardsDealtPerTurn = cardsDealtPerTurn;
        foreach(CardType cardType in startingDeck.cards)
        {
            cards.Add(new Card(cardType));
        }
    }
    
    public Deck(List<Card> cards)
    {
        this.startingDeck = new StartingDeck(cards);
        foreach(Card card in cards)
        {
            this.cards.Add(card);
        }
    }

    public void PurgeCard(string cardId)
    {
        cards.RemoveAll(card => card.id == cardId);
    }

    public void AddCards(Deck other) {
        foreach (var card in other.cards) {
            cards.Add(card);
        }
    }

}
