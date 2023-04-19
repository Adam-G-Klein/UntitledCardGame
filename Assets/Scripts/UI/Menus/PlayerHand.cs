using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[ExecuteInEditMode]
[RequireComponent(typeof(CardCastEventListener))]
[RequireComponent(typeof(CardDealtEventListener))]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class PlayerHand : GenericSingleton<PlayerHand>
{
    public List<PlayableCard> cardsInHand;

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private RectTransform layoutGroup;

    public void cardDealtEventHandler(CardsDealtEventInfo info){
        PlayableCard newCard;
        foreach(Card cardInfo in info.cards) {
            newCard = PrefabInstantiator.instantiateCard(
                cardPrefab, 
                layoutGroup, 
                cardInfo, 
                info.entityFrom);
            cardsInHand.Add(newCard);
        }
    }

    public void cardCastEventHandler(CardCastEventInfo info){
        PlayableCard cardToDiscard = null;
        foreach(PlayableCard card in cardsInHand) {
            if(card.card.id == info.cardInfo.id) {
                cardToDiscard = card;
            }
        }
        if(cardToDiscard != null) {
            discardCard(cardToDiscard);
        }
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
        if(info.newPhase == TurnPhase.END_PLAYER_TURN) {
            discardHand();
        }
    }

    private void discardHand(){
        List<PlayableCard> retainedCards = new List<PlayableCard>();
        foreach(PlayableCard card in cardsInHand) {
            if(card.retained) {
                retainedCards.Add(card);
                card.retained = false;
            } else {
                Destroy(card.gameObject);
                card.discardFromDeck();
            }
        }
        // do this instead of calling remove for each
        // to prevent enumeration issues in the for loop
        cardsInHand = retainedCards;
    }

    // Do not call on whole hand, only call on individual cards
    // modifies the list of cards in hand 
    public void discardCard(PlayableCard card){
        cardsInHand.Remove(card);
        Destroy(card.gameObject);
        card.discardFromDeck();
    }

    public void updateLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);

    }
}