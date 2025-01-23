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

    private GameObject cardPrefab;
    private GameObject cardDrawVFXPrefab;

    public delegate IEnumerator OnCardExhaustHandler(DeckInstance deckFrom, Card card);
    public event OnCardExhaustHandler onCardExhaustHandler;

    public delegate IEnumerator OnCardCastHandler(PlayableCard card);
    public event OnCardCastHandler onCardCastHandler;

    public delegate IEnumerator OnDeckShuffleHandler(DeckInstance deckFrom);
    public event OnDeckShuffleHandler onDeckShuffledHandler;

    private bool cardsInHandLocked = false;

    void Start() {
        cardPrefab = EnemyEncounterManager.Instance.encounterConstants.cardPrefab;
        cardDrawVFXPrefab = EnemyEncounterManager.Instance.encounterConstants.cardDrawVFXPrefab;
    }

    public void UpdatePlayableCards(DeckInstance deckFrom = null) {
        // TODO loop through all cards and update all cards from the related deck
        foreach(PlayableCard playableCard in cardsInHand) {
            if (null == deckFrom || playableCard.deckFrom == deckFrom) {
                playableCard.UpdateCardText();
            }
        };
    }

    public List<PlayableCard> DealCards(List<Card> cards, DeckInstance deckFrom) {
        List<PlayableCard> cardsDelt = new List<PlayableCard>();
        PlayableCard newCard;
        foreach(Card cardInfo in cards) {
            if(cardsInHand.Count >= GameplayConstantsSingleton.Instance.gameplayConstants.MAX_HAND_SIZE) {
                Debug.Log("PlayerHand: Hand is full, not dealing card");
                break;
            }
            WorldPositionVisualElement newCardPlacement = UIDocumentGameObjectPlacer.Instance.checkoutCardMapping();
            newCard = PrefabInstantiator.InstantiateCard(
                cardPrefab,
                EnemyEncounterManager.Instance.transform,
                cardInfo,
                deckFrom,
                newCardPlacement.worldPos);
            newCard.gameObject.name = cardInfo.name;
            newCard.interactable = false;
            UIDocumentGameObjectPlacer.Instance.addMapping(newCardPlacement, newCard.gameObject);
            if (newCard.card.cardType.retain) {
                newCard.retained = true;
            }
            cardsInHand.Add(newCard);
            cardsDelt.Add(newCard);
            CardDrawVFX(deckFrom.transform.position, newCardPlacement.worldPos, newCard.gameObject);
        }
        //PlayerHand.Instance.UpdatePlayableCards();
        return cardsDelt;
    }

    private void CardDrawVFX(Vector3 fromLocation, Vector3 toLocation, GameObject gameObject) {
        FXExperience experience = PrefabInstantiator.instantiateFXExperience(cardDrawVFXPrefab, Vector3.zero);

        experience.AddLocationToKey("companion", fromLocation);
        experience.AddLocationToKey("hand", toLocation);
        experience.BindGameObjectsToTracks(new Dictionary<string, GameObject>() {
            { "card", gameObject },
        });
        experience.StartExperience( () => {
            Debug.Log("Card draw VFX finished");
            gameObject.GetComponent<PlayableCard>().interactable = true;
        });
    }

    public void TurnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.END_PLAYER_TURN) {
            List<PlayableCard> retainedCards = new();
            // Run the effect workflows for all the cards left in the hand,
            // then destroy them with the callback.
            // The reason we want to use the callback is that otherwise, the
            // game objects are destroyed before the effect workflow can complete.
            cardsInHandLocked = true;
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
                    callback = DiscardFromHandCallback(card);
                }
                CardType ct = card.card.cardType;
                List<EffectStep> workflowSteps = new();
                if (ct.inPlayerHandEndOfTurnWorkflow != null) {
                    workflowSteps = ct.inPlayerHandEndOfTurnWorkflow.effectSteps;
                }
                EffectDocument document = new EffectDocument();
                document.map.AddItem(EffectDocument.ORIGIN, card.GetComponent<PlayableCard>());
                document.originEntityType = EntityType.Card;
                Debug.Log("Invoking effect workflow with steps: " + workflowSteps.Count);
                EffectManager.Instance.invokeEffectWorkflow(document, workflowSteps, callback);
            }
            cardsInHandLocked = false;

            cardsInHand = retainedCards;
        }
    }

    private IEnumerator DiscardFromHandCallback(PlayableCard card) {
        Debug.Log("PlayerHand: DiscardFromHandCallback for card: " + card.card);
        StartCoroutine(SafeRemoveCardFromHand(card));
        // Discarding from hand
        DiscardCard(card);
        if (card.gameObject.activeSelf) {
            card.DiscardToDeck();
        }
        yield return null;
    }

    IEnumerator SafeRemoveCardFromHand(PlayableCard card) {
        yield return new WaitUntil(() => !cardsInHandLocked);
        cardsInHand.Remove(card);
    }

    public void SafeRemoveCardFromHand(Card card) {
        StartCoroutine(SafeRemoveCardFromHand(GetCardById(card.id)));
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
        // If statements are here to take into account if a card exhausts itself
        // as part of its effect workflow
        if (cardsInHand.Contains(card)) {
            StartCoroutine(SafeRemoveCardFromHand(card));
        }
        // if(card.gameObject.activeSelf) {
        //     card.DiscardToDeck();
        // }
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
