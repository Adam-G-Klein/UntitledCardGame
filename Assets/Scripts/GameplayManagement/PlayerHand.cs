using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[ExecuteInEditMode]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class PlayerHand : GenericSingleton<PlayerHand>
{
    public List<PlayableCard> cardsInHand;

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private RectTransform layoutGroup;

    public List<PlayableCard> DealCards(List<Card> cards, DeckInstance deckFrom) {
        List<PlayableCard> cardsDelt = new List<PlayableCard>();
        PlayableCard newCard;
        foreach(Card cardInfo in cards) {
            newCard = PrefabInstantiator.InstantiateCard(
                cardPrefab, 
                layoutGroup, 
                cardInfo, 
                deckFrom);
            cardsInHand.Add(newCard);
            cardsDelt.Add(newCard);
        }
        return cardsDelt;
    }

    public void TurnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.END_PLAYER_TURN) {
            DiscardHand();
        }
    }

    private void DiscardHand() {
        List<PlayableCard> retainedCards = new List<PlayableCard>();
        foreach(PlayableCard card in cardsInHand) {
            if(card.retained) {
                retainedCards.Add(card);
                card.retained = false;
            } else {
                Destroy(card.gameObject);
                card.DiscardFromDeck();
            }
        }
        // do this instead of calling remove for each
        // to prevent enumeration issues in the for loop
        cardsInHand = retainedCards;
    }

    // Do not call on whole hand, only call on individual cards
    // modifies the list of cards in hand 
    public void DiscardCard(PlayableCard card) {
        // If statement is here to take into account if a card exhausts itself
        // as part of its effect workflow
        if (cardsInHand.Contains(card)) {
            cardsInHand.Remove(card);
            card.DiscardFromDeck();
        }
    }

    public void UpdateLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);

    }

    public PlayableCard GetCardById(string id) {
        foreach (PlayableCard card in cardsInHand) {
            if (card.card.id == id) {
                return card;
            }
        }
        Debug.LogError("PlayerHand: Unable to find card in hand with id " + id);
        return null;
    }
}