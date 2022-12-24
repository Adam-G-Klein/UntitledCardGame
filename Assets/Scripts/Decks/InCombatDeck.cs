using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class InCombatDeck
{
    public Deck sourceDeck;
    public List<CardInfo> drawPile;
    public List<CardInfo> discardPile;

    public InCombatDeck(Deck startingDeck)
    {
        this.sourceDeck = startingDeck;
        this.drawPile = new List<CardInfo>();
        this.drawPile.AddRange(startingDeck.cards);
        this.discardPile = new List<CardInfo>();
    }
    public InCombatDeck(List<CardInfo> cards)
    {
        this.sourceDeck = new Deck(cards);
        this.drawPile = new List<CardInfo>();
        this.drawPile.AddRange(cards);
        this.discardPile = new List<CardInfo>();
    }


    public List<CardInfo> dealCardsFromDeck(int numCards){
        List<CardInfo> returnList = new List<CardInfo>();
        CardInfo card;
        for(int i = 0; i < numCards; i++){
            //Drawing with replacement for now
            card = drawPile[Random.Range(0, drawPile.Count)];
            returnList.Add(card);
        }
        return returnList;
    }
}
