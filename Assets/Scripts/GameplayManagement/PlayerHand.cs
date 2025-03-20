using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using System.Linq;
using UnityEngine.Playables;


[ExecuteInEditMode]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class PlayerHand : GenericSingleton<PlayerHand>
{
    public List<PlayableCard> cardsInHand;

    private GameObject cardPrefab;
    private GameObject cardDrawVFXPrefab;

    public delegate IEnumerator OnCardExhaustHandler(DeckInstance deckFrom, Card card);
    public event OnCardExhaustHandler onCardExhaustHandler;

    public delegate IEnumerator OnCardDiscardHandler(DeckInstance deckFrom, Card card, bool casted);
    public event OnCardDiscardHandler onCardDiscardHandler;

    public delegate IEnumerator OnCardCastHandler(PlayableCard card);
    public event OnCardCastHandler onCardCastHandler;

    public delegate IEnumerator OnHandEmptyHandler();
    public event OnHandEmptyHandler onHandEmptyHandler;

    public delegate IEnumerator OnDeckShuffleHandler(DeckInstance deckFrom);
    public event OnDeckShuffleHandler onDeckShuffledHandler;

    private bool cardsInHandLocked = false;
    private Dictionary<GameObject, FXExperience> GOToFXExperience = new();
    private readonly float cardDealDelay = .1f;
    private bool canPlayCards = true;

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

            // okay so the real problem here is that createCardSlot updates the core UIDocument and we need to give it a few frames for all of the
            // newly created elements to understand where they are.
            // After the elements have been successfully updated AnimateCardsAfterLayout will be called and they'll start moving now that they know where
            // they need to go.
            Vector3 startPos = deckFrom.transform.position;
            WorldPositionVisualElement newCardPlacement = UIDocumentGameObjectPlacer.Instance.CreateCardSlot(() => {StartCoroutine(AnimateCardsAfterLayout(cardsDelt, startPos, .1f));});
            newCard = PrefabInstantiator.InstantiateCard(
                cardPrefab,
                EnemyEncounterManager.Instance.transform,
                cardInfo,
                deckFrom,
                startPos);
            newCard.gameObject.name = cardInfo.name;
            newCard.interactable = false;
            UIDocumentGameObjectPlacer.Instance.addMapping(newCardPlacement, newCard.gameObject);
            if (newCard.card.cardType.retain) {
                newCard.retained = true;
            }
            cardsInHand.Add(newCard);
            cardsDelt.Add(newCard);
        }

        return cardsDelt;
    }

    private IEnumerator AnimateCardsAfterLayout(List<PlayableCard> cardsDelt, Vector3 startPos, float delay = 0) {
        for (int i = 0; i < cardsInHand.Count; i++) {
            PlayableCard cardToMove = cardsInHand[i];
            cardToMove.interactable = false; // hovering a card changes its position so we really need that to not happen while they are moving to their new spot
            WorldPositionVisualElement WPVE = UIDocumentGameObjectPlacer.Instance.GetCardWPVEFromGO(cardToMove.gameObject);
            WPVE.UpdatePosition();
            CardDrawVFX(cardToMove.transform.position, WPVE.worldPos, cardToMove.gameObject);
            yield return new WaitForSeconds(delay);
        }
    }


    private void CardDrawVFX(Vector3 fromLocation, Vector3 toLocation, GameObject gameObject) {
        if (GOToFXExperience.ContainsKey(gameObject) && GOToFXExperience[gameObject] != null) {
            GOToFXExperience[gameObject].EarlyStop();
        }
        // TODO: update the FXExprience to do rotation as well, this was as much as I could muster rn.
        LeanTween.cancel(gameObject);
        LeanTween.rotate(gameObject, new Vector3(0, 0, UIDocumentGameObjectPlacer.Instance.GetCardWPVEFromGO(gameObject).ve.style.rotate.value.angle.value), .75f).setEase(LeanTweenType.easeInOutQuad);

        FXExperience experience = PrefabInstantiator.instantiateFXExperience(cardDrawVFXPrefab, Vector3.zero);
        GOToFXExperience[gameObject] = experience;

        experience.AddLocationToKey("companion", fromLocation);
        experience.AddLocationToKey("hand", toLocation);
        experience.BindGameObjectsToTracks(new Dictionary<string, GameObject>() {
            { "card", gameObject },
        });
        experience.StartExperience( () => {
            Debug.Log("Card draw VFX finished");
            if (gameObject.TryGetComponent<SpriteRenderer>(out var SR)) SR.sortingLayerName = "Cards"; // what is this magic
            gameObject.GetComponent<PlayableCard>().interactable = true;
            gameObject.GetComponent<PlayableCard>().SetBasePosition();
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
                    UIDocumentGameObjectPlacer.Instance.RemoveCardSlot(card.gameObject);
                    callback = DiscardCard(card);
                    if(NonMouseInputManager.Instance.inputMethod != InputMethod.Mouse) {
                        // Doesn't work, iteration to be done
                        //callback += NonMouseInputManager.Instance.hoverACard();
                    }
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

    public IEnumerator SafeRemoveCardFromHand(PlayableCard card) {
        yield return new WaitUntil(() => !cardsInHandLocked);
        cardsInHand.Remove(card);
    }

    public void SafeRemoveCardFromHand(Card card) {
        StartCoroutine(SafeRemoveCardFromHand(GetCardById(card.id)));
    }

    public IEnumerator OnCardExhaust(DeckInstance deckFrom, Card card) {
        if (onCardExhaustHandler != null) {
            foreach (OnCardExhaustHandler handler in onCardExhaustHandler.GetInvocationList()) {
                yield return handler.Invoke(deckFrom, card);
            }
        }
    }

    public IEnumerator OnCardDiscard(DeckInstance deckFrom, Card card, bool casted) {
        if (onCardDiscardHandler != null) {
            foreach (OnCardDiscardHandler handler in onCardDiscardHandler.GetInvocationList()) {
                yield return handler.Invoke(deckFrom, card, casted);
            }
        }
    }

    public IEnumerator OnHandEmpty() {
        if (onHandEmptyHandler != null) {
            Debug.Log("OnHandEmpty number of invocations: " + onHandEmptyHandler.GetInvocationList().Count());
            foreach (OnHandEmptyHandler handler in onHandEmptyHandler.GetInvocationList()) {
                yield return StartCoroutine(handler.Invoke());
            }
        }
    }

    // Do not call on whole hand, only call on individual cards
    // modifies the list of cards in hand
    public IEnumerator DiscardCard(PlayableCard card, bool cardCasted = false) {
        // If statements are here to take into account if a card exhausts itself
        // as part of its effect workflow
        if (cardsInHand.Contains(card)) {
            yield return StartCoroutine(SafeRemoveCardFromHand(card));
        }
        yield return OnCardDiscard(card.deckFrom, card.card, cardCasted);
        if(card.gameObject.activeSelf) {
            yield return card.DiscardToDeck();
        }
    }

    public IEnumerator ResizeHand(PlayableCard card) {
        UIDocumentGameObjectPlacer.Instance.RemoveCardSlot(card.gameObject, () => { StartCoroutine(AnimateCardsAfterLayout(new List<PlayableCard>(), new Vector3(), 0)); });
        yield return null;
    }

    // Do not call on whole hand, only call on individual cards
    // modifies the list of cards in hand
    public IEnumerator ExhaustCard(PlayableCard card) {
        card.CardExhaustVFX();
        // If statements are here to take into account if a card exhausts itself
        // as part of its effect workflow
        if (cardsInHand.Contains(card)) {
            yield return SafeRemoveCardFromHand(card);
        }
        if(card.gameObject.activeSelf) {
            yield return card.ExhaustCard();
        }
    }

    public PlayableCard GetCardById(string id) {
        foreach (PlayableCard card in cardsInHand) {
            if (card.card.id == id) {
                return card;
            }
        }
        Debug.LogWarning("PlayerHand: Unable to find card in hand with id " + id);
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

    public void DisableHand() {
        canPlayCards = false;
        foreach (PlayableCard card in cardsInHand) {
            card.interactable = false; // not sure why this wasn't enough honestly :/
        }
    }

    public bool GetCanPlayCards() {
        return canPlayCards;
    }
}
