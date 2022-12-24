using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Not currently in use
[CreateAssetMenu(
    fileName = "StartinDeck", 
    menuName = "Decks/Starting Deck")]
public class StartingDeck : ScriptableObject
{
    public List<CardInfo> cards;

    // Used by Deck(List<CardInfo>) constructor 
    // so that we can instantiate new decks
    // programmatically during runtime if we want
    // to create improving companions in the shop
    public StartingDeck(List<CardInfo> cards)
    {
        this.cards = cards;
    }
}
