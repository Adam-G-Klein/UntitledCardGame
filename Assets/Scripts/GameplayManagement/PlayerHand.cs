using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using System.Linq;
using UnityEngine.Playables;
using System;
using UnityEngine.Splines;


[ExecuteInEditMode]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class PlayerHand : GenericSingleton<PlayerHand>
{
    public List<PlayableCard> cardsInHand;
    public SplineContainer splineContainer;

    private GameObject cardPrefab;
    private GameObject cardDrawVFXPrefab;

    public delegate IEnumerator OnCardExhaustHandler(DeckInstance deckFrom, PlayableCard card);
    public event OnCardExhaustHandler onCardExhaustHandler;

    public delegate IEnumerator OnCardDiscardHandler(DeckInstance deckFrom, PlayableCard card, bool casted);
    public event OnCardDiscardHandler onCardDiscardHandler;

    public delegate IEnumerator OnCardCastHandler(PlayableCard card);
    public event OnCardCastHandler onCardCastHandler;
    public delegate IEnumerator OnAttackCardCastHandler(PlayableCard card);
    public event OnAttackCardCastHandler onAttackCardCastHandler;

    public delegate IEnumerator OnHandEmptyHandler();
    public event OnHandEmptyHandler onHandEmptyHandler;

    public delegate IEnumerator OnDeckShuffleHandler(DeckInstance deckFrom);
    public event OnDeckShuffleHandler onDeckShuffledHandler;

    private bool cardsInHandLocked = false;
    private Dictionary<GameObject, FXExperience> GOToFXExperience = new();
    private readonly float cardDealDelay = .05f;
    private bool canPlayCards = true;

    private List<PlayableCard> cardsToDeal = new List<PlayableCard>();
    private bool isBatchingCards = false;
    private float cardBatchingDelay = .1f;
    private int indexToHover = -1;
    private Coroutine dealingCardsCoroutine = null;
    private Dictionary<GameObject, int> cardToTweenMap = new();

    private Queue<PlayableCard> cardDealQueue = new Queue<PlayableCard>();

    private int MAX_HAND_SIZE;

    void Start() {
        cardPrefab = EnemyEncounterManager.Instance.encounterConstants.cardPrefab;
        cardDrawVFXPrefab = EnemyEncounterManager.Instance.encounterConstants.cardDrawVFXPrefab;
        MAX_HAND_SIZE = GameplayConstantsSingleton.Instance.gameplayConstants.MAX_HAND_SIZE;

        StartCoroutine(CardDealerWorker());
    }

    public void UpdatePlayableCards(DeckInstance deckFrom = null) {
        // TODO loop through all cards and update all cards from the related deck
        foreach(PlayableCard playableCard in cardsInHand) {
            if (null == deckFrom || playableCard.deckFrom == deckFrom) {
                playableCard.UpdateCardText();
            }
        };
    }

    private IEnumerator CardDealerWorker()
    {
        while (true)
        {
            // Idle if there are no cards to deal.
            if (cardDealQueue.Count == 0)
            {
                // Bus runs every half a second.
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            List<PlayableCard> cardsToBeShifted = new List<PlayableCard>(cardsInHand);
            List<PlayableCard> newCards = new List<PlayableCard>();
            // Invariant: once a card is present in the queue, we guarantee that card will be dealt.
            while (cardDealQueue.Count > 0)
            {
                PlayableCard newCard = cardDealQueue.Dequeue();

                Debug.Log($"[PLAYERHAND.DEAL] Dealing card {newCard.card.cardType.Name}");

                WorldPositionVisualElement newCardPlacement = UIDocumentGameObjectPlacer.Instance.CreateCardSlot();
                UIDocumentGameObjectPlacer.Instance.addMapping(newCardPlacement, newCard.gameObject);
                newCards.Add(newCard);
                cardsInHand.Add(newCard);

                // Wait to see if we catch any more cards in the batch to let more cards get on the bus.
                yield return new WaitForSeconds(cardBatchingDelay);
            }

            yield return UpdateCardPositions(newCards);

            // // Need to wait a frame for the UI layout system to place the wpve before dealing the card.
            // // This is necessary to do before shifting as well because the existing card wpves need to update.

            // // Shift the existing Cards to their slots.
            // for (int i = 0; i < cardsToBeShifted.Count; i++)
            // {
            //     // There is a chance that the thing that triggered the draw exhausts itself
            //     // and removes itself from cardsInHand before we get here.
            //     // Thus the null check helps us.
            //     if (cardsToBeShifted[i] == null) continue;
            //     ShiftCard(cardsToBeShifted[i]);
            // }

            // // Deal the new cards.
            // for (int i = 0; i < newCards.Count; i++)
            // {
            //     MoveCard(newCards[i]);
            //     yield return new WaitForSeconds(cardDealDelay);
            // }

            // Debug.Log($"[PLAYERHAND.DEAL] Done processing, now updating the playable cards");
            // yield return null;
            UpdatePlayableCards();

        }
    }

    private void ShiftAllCardsInHand()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            // There is a chance that the thing that triggered the draw exhausts itself
            // and removes itself from cardsInHand before we get here.
            // Thus the null check helps us.
            if (cardsInHand[i] == null) continue;
            ShiftCard(cardsInHand[i]);
        }
    }

    private IEnumerator UpdateCardPositions(List<PlayableCard> newCards = null) {
        if (cardsInHand.Count == 0) yield break;

        float cardSpacing = 1f / MAX_HAND_SIZE;
        float firstCardPosition = 0.5f - (cardsInHand.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < cardsInHand.Count; i++) {
            float p = firstCardPosition + (i * cardSpacing);
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 editedPosition = new Vector3(splinePosition.x, splinePosition.y, 0f);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);
            bool isNew = newCards.Contains(cardsInHand[i]);
            yield return MoveSingleCard(cardsInHand[i], editedPosition, rotation, isNew);
    }}

    private IEnumerator MoveSingleCard(PlayableCard cardToMove, Vector3 position, Quaternion rotation, bool isNewCard) {
        if (isNewCard) cardToMove.gameObject.SetActive(true);
        GameObject moveGO = cardToMove.transform.parent.gameObject;
        GameObject rotateGO = cardToMove.gameObject;
        float rotationTime = 0.25f;
        float moveTime = 0.25f;

        LeanTween.rotate(rotateGO, rotation.eulerAngles, rotationTime)
                // .setOnComplete(() => { cardToTweenMap.Remove(rotateGO); })
                .setEase(LeanTweenType.easeInOutQuad);

        LeanTween.move(moveGO, position, moveTime)
                .setOnComplete(() => { 
                    if (!isNewCard) return;

                    if (cardToMove.TryGetComponent<SpriteRenderer>(out var SR)) SR.sortingLayerName = "Cards"; // what is this magic
                    cardToMove.interactable = true;

                    if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;

                    if (cardsInHand.IndexOf(cardToMove) == indexToHover) {
                        if (cardToMove.TryGetComponent<GameObjectFocusable>(out GameObjectFocusable goFocusable)) {
                            FocusManager.Instance.SetFocus(goFocusable);
                        }
                        indexToHover = -1;
                        return;
                    }
                })
                .setEase(LeanTweenType.easeInOutQuad);

        yield return new WaitForSeconds(cardDealDelay);
    }

    private List<PlayableCard> DealCardsQueued(List<Card> cards, DeckInstance deckFrom)
    {
        List<PlayableCard> cardsEnqueued = new List<PlayableCard>();

        foreach (Card cardInfo in cards)
        {
            if (cardsInHand.Count + cardDealQueue.Count >= GameplayConstantsSingleton.Instance.gameplayConstants.MAX_HAND_SIZE)
            {
                Debug.Log("PlayerHand: Hand is full, not queuing card");
                break;
            }

            Vector3 startPos = deckFrom.transform.position;
            PlayableCard newCard = PrefabInstantiator.InstantiateCard(
                cardPrefab,
                EnemyEncounterManager.Instance.transform,
                cardInfo,
                deckFrom,
                startPos);
            newCard.gameObject.name = cardInfo.name;
            newCard.interactable = false;
            newCard.gameObject.SetActive(false);
            if (newCard.card.cardType.retain)
            {
                newCard.retained = true;
            }

            // Just enqueue the card (we'll fully instantiate when dealing)
            cardDealQueue.Enqueue(newCard);
            // Optionally track which were enqueued
            // (You might want to return instantiated PlayableCards later instead)
            cardsEnqueued.Add(newCard);

            Debug.Log($"[PLAYERHAND.DEAL] Enqueuing card {newCard.card.cardType.Name}");
        }

        // Ensure only one coroutine is running
        // if (dealCoroutine == null)
        // {
        //     dealCoroutine = StartCoroutine(ProcessDealQueue());
        // }

        return cardsEnqueued;
    }

    public List<PlayableCard> DealCards(List<Card> cards, DeckInstance deckFrom)
    {
        return DealCardsQueued(cards, deckFrom);
    }

    private void ShiftCard(PlayableCard cardToShift) {
        WorldPositionVisualElement WPVE = UIDocumentGameObjectPlacer.Instance.GetCardWPVEFromGO(cardToShift.gameObject);
        WPVE.UpdatePosition();
        GameObject moveGO = cardToShift.transform.parent.gameObject;
        GameObject rotateGO = cardToShift.gameObject;

        float rotationTime = 0.25f;
        float moveTime = 0.25f;

        // The card was already moving and we wanna change it to move somehow else
        if (cardToTweenMap.ContainsKey(moveGO)) {
            LTDescr descr = LeanTween.descr(cardToTweenMap[moveGO]);
            if (descr == null) {
                // This should be an abnormal case
                cardToTweenMap.Remove(moveGO);
            } else {
                // This should be the happy path
                moveTime = descr.time - descr.passed;
                LeanTween.cancel(moveGO);
            }
        }

        if (cardToTweenMap.ContainsKey(rotateGO)) {
            LTDescr descr = LeanTween.descr(cardToTweenMap[rotateGO]);
            if (descr == null) {
                // This should be an abnormal case
                cardToTweenMap.Remove(moveGO);
            } else {
                // This should be the happy path
                rotationTime = descr.time - descr.passed;
                LeanTween.cancel(rotateGO);
            }
        }

        // The card isn't already moving :)
        int rotation = LeanTween.rotate(
                rotateGO,
                new Vector3(0, 0, UIDocumentGameObjectPlacer.Instance.GetCardWPVEFromGO(rotateGO).ve.style.rotate.value.angle.value),
                rotationTime
                ).setOnComplete(() => { cardToTweenMap.Remove(rotateGO); })
                .setEase(LeanTweenType.easeInOutQuad).id;

        int move = LeanTween.move(
                moveGO,
                WPVE.worldPos,
                moveTime
                ).setOnComplete(() => { cardToTweenMap.Remove(moveGO); })
                .setEase(LeanTweenType.easeInOutQuad).id;

        cardToTweenMap[rotateGO] = rotation;
        cardToTweenMap[moveGO] = move;
    }

    private void MoveCard(PlayableCard cardToMove, bool disableCardDuringMove = false) {
        if (disableCardDuringMove) cardToMove.interactable = false; // hovering a card changes its position so we really need that to not happen while they are moving to their new spot
        WorldPositionVisualElement WPVE = UIDocumentGameObjectPlacer.Instance.GetCardWPVEFromGO(cardToMove.gameObject);
        WPVE.UpdatePosition();
        Debug.Log($"[PLAYERHAND.DEAL] World pos target for card {cardToMove.card.cardType.Name} is {WPVE.worldPos}");
        CardDrawVFX(cardToMove.transform.parent.transform.position, WPVE.worldPos, cardToMove.gameObject);
    }


    private void CardDrawVFX(Vector3 fromLocation, Vector3 toLocation, GameObject gameObject) {
        gameObject.SetActive(true);
        // This code is being deprecated as of a week before magwest.
        // If this code remains deprecated post mag west it can be deleted.
        // if (GOToFXExperience.ContainsKey(gameObject) && GOToFXExperience[gameObject] != null) {
        //     FXExperience ex = GOToFXExperience[gameObject];
        //     ex.UpdateLocationKey("hand", toLocation);
        //     LeanTween.cancel(gameObject);
        //     PlayableDirector director = ex.playableDirector;
        //     float timeRemaining = (float)(director.duration - director.time);
        //     LeanTween.rotate(gameObject, new Vector3(0, 0, UIDocumentGameObjectPlacer.Instance.GetCardWPVEFromGO(gameObject).ve.style.rotate.value.angle.value), timeRemaining).setEase(LeanTweenType.easeInOutQuad);
        //     return;
        // }

        // TODO: update the FXExprience to do rotation as well, this was as much as I could muster rn.
        LeanTween.cancel(gameObject);
        LeanTween.rotate(gameObject, new Vector3(0, 0, UIDocumentGameObjectPlacer.Instance.GetCardWPVEFromGO(gameObject).ve.style.rotate.value.angle.value), .75f).setEase(LeanTweenType.easeInOutQuad);

        FXExperience experience = PrefabInstantiator.instantiateFXExperience(cardDrawVFXPrefab, Vector3.zero);
        GOToFXExperience[gameObject] = experience;

        gameObject.GetComponent<PlayableCard>().SetBasePosition(toLocation);
        experience.AddLocationToKey("companion", fromLocation);

        experience.AddLocationToKey("hand", toLocation);
        experience.BindGameObjectsToTracks(new Dictionary<string, GameObject>() {
            { "card", gameObject.transform.parent.gameObject },
        });

        experience.StartExperience( () => {
            Debug.Log("Card draw VFX finished");
            if (gameObject.TryGetComponent<SpriteRenderer>(out var SR)) SR.sortingLayerName = "Cards"; // what is this magic
            gameObject.GetComponent<PlayableCard>().interactable = true;

            if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;

            if (cardsInHand.IndexOf(gameObject.GetComponent<PlayableCard>()) == indexToHover) {
                if (cardsInHand[indexToHover].TryGetComponent<GameObjectFocusable>(out GameObjectFocusable goFocusable)) {
                    FocusManager.Instance.SetFocus(goFocusable);
                }
                indexToHover = -1;
                return;
            }
        });
    }

    public void StopCardDrawFX(GameObject gameObject) {
        LeanTween.cancel(gameObject);
        // if (GOToFXExperience.ContainsKey(gameObject) && GOToFXExperience[gameObject] != null) {
        //     FXExperience ex = GOToFXExperience[gameObject];
        //     ex.EarlyStop();
        //     GOToFXExperience.Remove(gameObject);
        // }
    }

    public void HoverNextCard(int previouslyPlayedCardIndex) {
        if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;

        if (cardsInHand.Count == 0) {
            indexToHover = 0;
            return;
        }
        indexToHover = -1;
        int nextHoverIndex = cardsInHand.Count <= previouslyPlayedCardIndex ? 0 : previouslyPlayedCardIndex;

        if (cardsInHand[nextHoverIndex].TryGetComponent<GameObjectFocusable>(out GameObjectFocusable goFocusable)) {
            FocusManager.Instance.SetFocus(goFocusable);
        }

    }

    public void TurnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
        if (info.newPhase == TurnPhase.PLAYER_TURN) {
            canPlayCards = true;
            indexToHover = 0;
        }
        if(info.newPhase == TurnPhase.END_PLAYER_TURN) {
            canPlayCards = false;
            List<PlayableCard> retainedCards = new();
            // Run the effect workflows for all the cards left in the hand,
            // then destroy them with the callback.
            // The reason we want to use the callback is that otherwise, the
            // game objects are destroyed before the effect workflow can complete.
            cardsInHandLocked = true;
            indexToHover = 0;
            List<PlayableCard> cardsToImmediatelyDiscard = new();
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
                    StartCoroutine(ResizeHand(card));
                    callback = DiscardCard(card, true);
                }
                CardType ct = card.card.cardType;

                if (ct.inPlayerHandEndOfTurnWorkflow != null && ct.inPlayerHandEndOfTurnWorkflow.effectSteps.Count > 0) {
                    List<EffectStep> workflowSteps = ct.inPlayerHandEndOfTurnWorkflow.effectSteps;
                    EffectDocument document = new EffectDocument();
                    document.map.AddItem(EffectDocument.ORIGIN, card.GetComponent<PlayableCard>());
                    document.originEntityType = EntityType.Card;
                    Debug.Log("Invoking end of turn effect workflow with steps: " + workflowSteps.Count);
                    EffectManager.Instance.invokeEffectWorkflow(document, workflowSteps, callback);
                } else if (!retainedCards.Contains(card)) {
                    cardsToImmediatelyDiscard.Add(card);
                }
            }
            foreach (PlayableCard card in cardsToImmediatelyDiscard) {
                StartCoroutine(DiscardCard(card, true));
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

    public IEnumerator OnCardExhaust(DeckInstance deckFrom, PlayableCard card) {
        if (onCardExhaustHandler != null) {
            foreach (OnCardExhaustHandler handler in onCardExhaustHandler.GetInvocationList()) {
                yield return handler.Invoke(deckFrom, card);
            }
        }
    }

    public IEnumerator OnCardDiscard(DeckInstance deckFrom, PlayableCard card, bool casted) {
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
    public IEnumerator DiscardCard(PlayableCard card, bool cardCasted = false)
    {
        // If statements are here to take into account if a card exhausts itself
        // as part of its effect workflow
        if (!cardCasted && card.card.cardType.onDiscardEffectWorkflow != null)
        {
            EffectDocument document = new EffectDocument();
            document.originEntityType = EntityType.Card;
            if (card != null) document.map.AddItem<PlayableCard>(EffectDocument.ORIGIN, card);
            EffectManager.Instance.QueueEffectWorkflow(
                new EffectWorkflowClosure(document, card.card.cardType.onDiscardEffectWorkflow, null)
            );
        }
        if (card.gameObject.activeSelf)
        {
            if (!cardCasted && card.card.cardType.onDiscardEffectWorkflow != null && card.card.cardType.onDiscardEffectWorkflow.effectSteps.Count != 0)
            {
                EffectManager.Instance.QueueEffectWorkflow(
                    new EffectWorkflowClosure(new EffectDocument(), new EffectWorkflow(), DiscardCardAndNotifyDiscardHandlers(card, cardCasted))
                );
            }
            else
            {
                StartCoroutine(DiscardCardAndNotifyDiscardHandlers(card, cardCasted));
            }
        }
        yield return null;
    }

    public IEnumerator DiscardCardAndNotifyDiscardHandlers(PlayableCard card, bool cardCasted)
    {
        if (!cardCasted) yield return ResizeHand(card);
        StartCoroutine(card.DiscardToDeck());
        yield return OnCardDiscard(card.deckFrom, card, cardCasted);
    }

    public IEnumerator ResizeHand(PlayableCard card)
    {
        UIDocumentGameObjectPlacer.Instance.RemoveCardSlot(card.gameObject, () =>
        {
            // Ensure only one coroutine is running
            // if (dealCoroutine == null) {
            //     dealCoroutine = StartCoroutine(ProcessDealQueue());
            // }
            ShiftAllCardsInHand();
        });
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
        yield return ResizeHand(card);

        if (card.card.cardType.onExhaustEffectWorkflow != null) {
            EffectDocument document = new EffectDocument();
            document.originEntityType = EntityType.Card;
            if (card != null) document.map.AddItem<PlayableCard>(EffectDocument.ORIGIN, card);
            EffectManager.Instance.QueueEffectWorkflow(
                new EffectWorkflowClosure(document, card.card.cardType.onExhaustEffectWorkflow, null)
            );
        }
        yield return OnCardExhaust(card.deckFrom, card);
        // Queue up the callback last before returning execution so the callback will destroy the card
        // ONLY after the on exhaust workflows.
        // Any workflows queued by `deckFrom.ExhaustCard` will run BEFORE this.
        if(card.gameObject.activeSelf) {
            EffectManager.Instance.QueueEffectWorkflow(
                new EffectWorkflowClosure(new EffectDocument(), new EffectWorkflow(), card.ExhaustCard())
            );
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

    public IEnumerator OnCardCast(PlayableCard card)
    {
        if (onCardCastHandler != null)
        {
            foreach (OnCardCastHandler handler in onCardCastHandler.GetInvocationList())
            {
                yield return StartCoroutine(handler.Invoke(card));
            }
        }
        if (onAttackCardCastHandler != null && card.card.cardType.cardCategory == CardCategory.Attack)
        {
            foreach (OnAttackCardCastHandler handler in onAttackCardCastHandler.GetInvocationList())
            {
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

    public void FocusACard(PlayableCard notThisOne) {
        // filter cards by whether they're playable
        List<PlayableCard> playableCards = new List<PlayableCard>();
        foreach(PlayableCard card in cardsInHand) {
            if((notThisOne == null || notThisOne != card) && card.card.cardType.playable) {
                playableCards.Add(card);
            }
        }

        // filter cards by whether we have enough mana to play them
        List<PlayableCard> affordableCards = new List<PlayableCard>();
        foreach(PlayableCard card in playableCards) {
            if(card.card.GetManaCost() <= ManaManager.Instance.currentMana) {
                affordableCards.Add(card);
            }
        }
        if(affordableCards.Count > 0) {
            Debug.Log("[NonMouseInputManager] Found a playable and affordable card, hovering card: " + affordableCards[0].name);
            if (affordableCards[0].TryGetComponent<GameObjectFocusable>(out GameObjectFocusable goFocusable)) {
                FocusManager.Instance.SetFocus(goFocusable);
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
