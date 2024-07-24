using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;


[ExecuteInEditMode]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class PlayerHand : GenericSingleton<PlayerHand>
{
    public List<PlayableCard> cardsInHand;

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private RectTransform layoutGroup;

    public delegate IEnumerator OnCardExhaustHandler(DeckInstance deckFrom, Card card);
    public event OnCardExhaustHandler onCardExhaustHandler;

    public delegate IEnumerator OnCardCastHandler(PlayableCard card);
    public event OnCardCastHandler onCardCastHandler;

    public delegate IEnumerator OnDeckShuffleHandler(DeckInstance deckFrom);
    public event OnDeckShuffleHandler onDeckShuffledHandler;

    public List<PlayableCard> DealCards(List<Card> cards, DeckInstance deckFrom) {
        List<PlayableCard> cardsDelt = new List<PlayableCard>();
        PlayableCard newCard;
        foreach(Card cardInfo in cards) {
            newCard = PrefabInstantiator.InstantiateCard(
                cardPrefab,
                layoutGroup,
                cardInfo,
                deckFrom);
            if (newCard.card.cardType.retain) {
                newCard.retained = true;
            }
            cardsInHand.Add(newCard);
            cardsDelt.Add(newCard);
        }
        return cardsDelt;
    }

    public void TurnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.END_PLAYER_TURN) {
            List<PlayableCard> retainedCards = new();
            // Run the effect workflows for all the cards left in the hand,
            // then destroy them with the callback.
            // The reason we want to use the callback is that otherwise, the
            // game objects are destroyed before the effect workflow can complete.
            foreach (PlayableCard card in cardsInHand) {
                IEnumerator callback = null;
                // Do not destroy the card if it is retained.
                if (card.retained) {
                    retainedCards.Add(card);
                    // If the retain is a temporary effect and not innate to the card,
                    // remove the "retain".
                    if (!card.card.cardType.retain) {
                        card.retained = false;
                    }
                } else {
                    callback = DiscardAndDestroyCallback(card);
                }
                CardType ct = card.card.cardType;
                List<EffectStep> workflowSteps = new();
                if (ct.inPlayerHandEndOfTurnWorkflow != null) {
                    workflowSteps = ct.inPlayerHandEndOfTurnWorkflow.effectSteps;
                }
                EffectDocument document = new EffectDocument();
                document.map.AddItem(EffectDocument.ORIGIN, card.GetComponent<PlayableCard>());
                document.originEntityType = EntityType.Card;
                EffectManager.Instance.invokeEffectWorkflow(document, workflowSteps, callback);
            }

            cardsInHand = retainedCards;
        }
    }

    private IEnumerator DiscardAndDestroyCallback(PlayableCard card) {
        card.DiscardFromDeck();
        Destroy(card.gameObject);
        yield return null;
    }

    public void RemoveCardFromHand(Card card) {
        cardsInHand.Remove(GetCardById(card.id));
        UpdateLayout();
    }

    public IEnumerator OnCardExhaust(DeckInstance deckFrom, Card card) {
        if (onCardExhaustHandler != null) {
            foreach (OnCardExhaustHandler handler in onCardExhaustHandler.GetInvocationList()) {
                yield return StartCoroutine(handler.Invoke(deckFrom, card));
            }
        }
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
        UpdateLayout();
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

    public IEnumerator OnCardCast(PlayableCard card) {
        if (onCardCastHandler != null) {
            foreach (OnCardCastHandler handler in onCardCastHandler.GetInvocationList()) {
                yield return StartCoroutine(handler.Invoke(card));
            }
        }
    }

    public IEnumerator OnDeckShuffled(DeckInstance deck) {
        if (onDeckShuffledHandler != null) {
            foreach (OnDeckShuffleHandler handler in onDeckShuffledHandler.GetInvocationList()) {
                yield return StartCoroutine(handler.Invoke(deck));
            }
        }
    }
}
