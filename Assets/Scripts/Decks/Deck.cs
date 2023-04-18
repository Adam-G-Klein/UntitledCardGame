using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Deck
{
    public StartingDeck startingDeck;
    [SerializeField]
    public List<Card> cards = new List<Card>();

    public Deck(StartingDeck startingDeck)
    {
        this.startingDeck = startingDeck;
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

    public void purgeCard(string cardId)
    {
        cards.RemoveAll(card => card.id == cardId);
    }

}
