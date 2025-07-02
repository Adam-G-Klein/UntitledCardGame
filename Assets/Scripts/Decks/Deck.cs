using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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

    public Deck(DeckSerializable deckSerializable, SORegistry registry)
    {
        this.startingDeck = registry.GetAsset<StartingDeck>(deckSerializable.startingDeckGuid);
        this.cardsDealtPerTurn = deckSerializable.cardsDealtPerTurn;
        this.cards = deckSerializable.cards.Select(c => new Card(c, registry)).ToList();

    }

    public Deck(List<Card> cards, int initialCardsDealtPerTurn = 1)
    {
        this.startingDeck = new StartingDeck(cards);
        this.cardsDealtPerTurn = initialCardsDealtPerTurn;
        foreach(Card card in cards)
        {
            this.cards.Add(card);
        }
    }

    public void PurgeCard(string cardId)
    {
        cards.RemoveAll(card => card.id == cardId);
    }

     public void TransformAllCardsOfType(CardType targetCardType, CardType cardTypeToTransformInto) {
        Debug.Log("Transforming all cards of type" + targetCardType.name + " from deck into type " + cardTypeToTransformInto.name);
        List<Card> transformedDeck = new();
        foreach (Card card in cards) {
            if (card.cardType == targetCardType) {
                transformedDeck.Add(new Card(cardTypeToTransformInto, card.getCompanionFrom()));
            } else {
                transformedDeck.Add(card);
            }
        }
        cards = transformedDeck;
    }

    public void AddCards(Deck other) {
        foreach (var card in other.cards) {
            cards.Add(card);
        }
    }
}

[System.Serializable]
public class DeckSerializable
{
    public string startingDeckGuid;
    public int cardsDealtPerTurn;
    public List<CardSerializable> cards;

    public DeckSerializable(Deck deck)
    {
        this.startingDeckGuid = deck.startingDeck.GUID;
        this.cardsDealtPerTurn = deck.cardsDealtPerTurn;
        this.cards = deck.cards.Select(c => new CardSerializable(c)).ToList();
    }
}