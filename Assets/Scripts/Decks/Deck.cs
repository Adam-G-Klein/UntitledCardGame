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
    public Deck(CompanionTypeSO companionType)
    {
        this.startingDeck = companionType.startingDeck;
        this.cardsDealtPerTurn = companionType.initialCardsDealtPerTurn;
        foreach(CardType cardType in companionType.startingDeck.cards)
        {
            cards.Add(new Card(cardType, companionType));
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
