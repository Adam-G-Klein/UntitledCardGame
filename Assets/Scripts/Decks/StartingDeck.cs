using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StartinDeck", 
    menuName = "Decks/Starting Deck")]
public class StartingDeck : IdentifiableSO
{
    public List<CardType> cards = new();

    // Used by Deck(List<CardInfo>) constructor 
    // so that we can instantiate new decks
    // programmatically during runtime if we want
    // to create improving companions in the shop
    public StartingDeck(List<CardType> cards)
    {
        this.cards = cards;
    }

    public StartingDeck(List<Card> cards)
    {
        foreach(Card card in cards)
        {
            this.cards.Add(card.cardType);
        }
    }

}
