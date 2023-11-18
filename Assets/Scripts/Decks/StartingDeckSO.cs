using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Not currently in use
[CreateAssetMenu(
    fileName = "StartingDeckSO", 
    menuName = "Decks/Starting Deck")]
public class StartingDeckSO : ScriptableObject
{
    public List<CardType> cards;

    // Used by Deck(List<CardInfo>) constructor 
    // so that we can instantiate new decks
    // programmatically during runtime if we want
    // to create improving companions in the shop
    public StartingDeckSO(List<CardType> cards)
    {
        this.cards = cards;
    }

    public StartingDeckSO(List<Card> cards)
    {
        foreach(Card card in cards)
        {
            this.cards.Add(card.cardType);
        }
    }

}
