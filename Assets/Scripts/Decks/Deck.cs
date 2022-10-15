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
        this.startingDeck = startingDeck;
        this.cards = new List<CardInfo>();
        this.cards.AddRange(startingDeck.cards);
    }
}
