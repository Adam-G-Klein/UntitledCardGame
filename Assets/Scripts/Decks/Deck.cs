using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Deck
{
    public StartingDeck startingDeck;
    public List<CardInfo> cards;

    public Deck(StartingDeck startingDeck)
    {
        this.cards.AddRange(startingDeck.cards);
    }
    public Deck(List<CardInfo> cards)
    {
        this.startingDeck = new StartingDeck(cards);
        this.cards.AddRange(startingDeck.cards);
    }

}
