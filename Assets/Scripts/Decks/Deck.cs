using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Deck",
    menuName = "Decks/TestDeck")]
public class Deck: ScriptableObject
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
