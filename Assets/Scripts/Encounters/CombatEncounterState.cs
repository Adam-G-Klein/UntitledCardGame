using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatEncounterState
{
    public List<Card> cardsCastThisTurn = new List<Card>();
    public List<Card> cardsCastThisCombat = new List<Card>();
    public List<Card> cardsExhaustThisTurn = new List<Card>();
    public List<Card> cardsExhaustThisCombat = new List<Card>();
    public List<Card> cardsDiscardedThisTurn = new List<Card>();
    public List<Card> cardsDiscardedThisCombat = new List<Card>();
    public List<DeckInstance> decksShuffledThisCombat = new List<DeckInstance>();
    public int turn = 1;

    public void CastCard(Card card)
    {
        cardsCastThisTurn.Add(card);
    }

    public void ExhaustCard(Card card)
    {
        cardsExhaustThisTurn.Add(card);
    }

    public void DiscardCard(Card card)
    {
        cardsDiscardedThisTurn.Add(card);
    }


    public void UpdateStateOnEndTurn()
    {
        turn++;
        cardsCastThisCombat.AddRange(cardsCastThisTurn);
        cardsCastThisTurn = new List<Card>();
        cardsExhaustThisCombat.AddRange(cardsExhaustThisTurn);
        cardsExhaustThisTurn = new List<Card>();
        cardsDiscardedThisCombat.AddRange(cardsDiscardedThisTurn);
        cardsDiscardedThisTurn = new List<Card>();
    }

    public Card GetLastCastCard()
    {
        if (cardsCastThisTurn.Count == 0)
        {
            return null;
        }
        return cardsCastThisTurn[cardsCastThisTurn.Count - 1];
    }

    public void DeckShuffled(DeckInstance deckInstance)
    {
        decksShuffledThisCombat.Add(deckInstance);
    }

    public int GetNumberOfDecksShuffled()
    {
        return decksShuffledThisCombat.Count;
    }

    public int GetNumCardsOfCategoryPlayedThisTurn(CardCategory category)
    {
        int count = 0;
        foreach (Card card in cardsCastThisTurn)
        {
            if (card.cardType.cardCategory == category)
            {
                count += 1;
            }
        }
        return count;
    }
}
