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
    public List<DeckInstance> decksShuffledThisCombat = new List<DeckInstance>();

    public void CastCard(Card card) {
        cardsCastThisTurn.Add(card); 
    }

    public void ExhaustCard(Card card) {
        cardsExhaustThisTurn.Add(card);
    }
    
    public void UpdateStateOnEndTurn() {
        cardsCastThisCombat.AddRange(cardsCastThisTurn);
        cardsCastThisTurn = new List<Card>();
        cardsExhaustThisCombat.AddRange(cardsExhaustThisTurn);
        cardsExhaustThisTurn = new List<Card>();
    }

    public Card GetLastCastCard() {
        return cardsCastThisTurn[cardsCastThisTurn.Count];
    }

    public void DeckShuffled(DeckInstance deckInstance) {
        decksShuffledThisCombat.Add(deckInstance);
    }

    public int GetNumberOfDecksShuffled() {
        return decksShuffledThisCombat.Count;
    }
}
