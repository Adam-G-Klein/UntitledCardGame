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
using Unity.VisualScripting;


[ExecuteInEditMode]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class PlayerHand : GenericSingleton<PlayerHand>
{
    public List<PlayableCard> cardsInHand;
    public SplineContainer splineContainer;
    /*
        This curve is used to get what percent a card should move based on proximity
        to the current card being hovered. A card in hand directly next to the card being hovered
        should move out of the way much more than a card that is 4 cards away at the end of the hand.
        This doesn't actually control the "base" amount the cards move, that is the curve below.
    */
    public AnimationCurve distanceMovementCurve;
    /*
        This curve is used to determine how much "base" movement cards will move out
        of the way when a card is hovered. This curve is necessary because when you
        have a low amount of cards in hand, the cards are already spaced out enough that
        hovering left and right is easy / the cards are big enough to easily receive the
        pointer input. When you have 8-10 cards in hand, without giving more space when
        a card is hovered, moving your mouse from the hovered card to the card immediately
        next to it becomes really hard because there's only like 25% of the width of the card
        showing that can receive the mouse on pointer enter event.
    */
    public AnimationCurve extraDistanceHoverCurve;
    [SerializeField]
    private float hoverScale;
    [SerializeField]
    private float nonHoverScale;
    [SerializeField]
    private float hoverYOffset;
    [SerializeField]
    private float hoverZOffset;
    [SerializeField]
    private float hoverAnimationTime;


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

    private Queue<(PlayableCard, bool)> cardDealQueue = new Queue<(PlayableCard, bool)>();

    public delegate IEnumerator OnCardDrawHandler(PlayableCard card);
    public event OnCardDrawHandler onCardDrawHandler;

    private int MAX_HAND_SIZE;
    /*
        This int designates the scaling point for cards in hand. With a value of 7,
        this means that even if there are less cards in hand than 7, the the cards will
        be spaced out in the hand as if there were 7 cards in hand. So with any number
        of cards from 2 to 7, card spacing will be the same. Once we exceed 7 cards in hand,
        the space between cards starts decreasing in order to fit the larger amount of cards.
    */
    private int HAND_START_SCALING = 7;
    private PlayableCard hoveredCard = null;
    private float splineMiddleYpos;

    void Start() {
        cardPrefab = EnemyEncounterManager.Instance.encounterConstants.cardPrefab;
        cardDrawVFXPrefab = EnemyEncounterManager.Instance.encounterConstants.cardDrawVFXPrefab;
        MAX_HAND_SIZE = GameplayConstantsSingleton.Instance.gameplayConstants.MAX_HAND_SIZE;
        splineMiddleYpos = splineContainer.Spline[1].Position.y;

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
                // Bus runs every interval.
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            List<PlayableCard> cardsToBeShifted = new List<PlayableCard>(cardsInHand);
            List<(PlayableCard, bool)> newCards = new List<(PlayableCard, bool)>();
            List<PlayableCard> newCardsWithoutTheBoolField = new List<PlayableCard>();
            // Invariant: once a card is present in the queue, we guarantee that card will be dealt.
            while (cardDealQueue.Count > 0)
            {
                var x = cardDealQueue.Dequeue();
                PlayableCard newCard = x.Item1;
                bool countAsDraw = x.Item2;

                Debug.Log($"[PLAYERHAND.DEAL] Dealing card {newCard.card.cardType.GetName()}");

                WorldPositionVisualElement newCardPlacement = UIDocumentGameObjectPlacer.Instance.CreateCardSlot();
                UIDocumentGameObjectPlacer.Instance.addMapping(newCardPlacement, newCard.gameObject);
                newCards.Add(x);
                newCardsWithoutTheBoolField.Add(x.Item1);
                cardsInHand.Add(x.Item1);

                // Wait to see if we catch any more cards in the batch to let more cards get on the bus.
                yield return new WaitForSeconds(cardBatchingDelay);
            }

            UpdateCardPositions(newCardsWithoutTheBoolField);

            // process the OnDrawEvents for all the new cards with "countAsDraw" active.
            for (int i = 0; i < newCards.Count; i++)
            {
                if (!newCards[i].Item2) continue;
                yield return OnCardDraw(newCards[i].Item1);
            }

            Debug.Log($"[PLAYERHAND.DEAL] Done processing, now updating the playable cards");
            UpdatePlayableCards();
        }
    }

    public void HoverCard(PlayableCard card) {
        hoveredCard = card;
        LeanTween.scale(card.gameObject, new Vector3(hoverScale, hoverScale, 1), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
        LeanTween.moveLocal(card.gameObject, new Vector3(0, hoverYOffset, hoverZOffset), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
        UpdateCardPositions(null);
    }

    public void UnhoverCard(PlayableCard card) {
        hoveredCard = null;
        LeanTween.scale(card.gameObject, new Vector3(nonHoverScale, nonHoverScale, 1), hoverAnimationTime)
                .setEase(LeanTweenType.easeOutQuint);
        LeanTween.moveLocal(card.gameObject, new Vector3(0, 0, 0), hoverAnimationTime)
                .setEase(LeanTweenType.easeOutQuint);
        UpdateCardPositions();
    }

    // Function that updates all card positions
    // Optionally takes a list of new cards to dictate if a card is being newly delt, or just shifting in the hand
    // Optionally takes a playable card, which indicates a card to give extra room in the hand to
    private void UpdateCardPositions(List<PlayableCard> newCards = null) {
        if (cardsInHand.Count == 0) return;

        float minCardSpacing = 1f / MAX_HAND_SIZE;
        float maxCardSpacing = 1f / HAND_START_SCALING;
        float cardSpacing = Mathf.Clamp(1f / cardsInHand.Count, minCardSpacing, maxCardSpacing);
        int hoveredIndex = cardsInHand.IndexOf(hoveredCard);
        float firstCardPosition = 0.5f - (cardsInHand.Count - 1) * (cardSpacing / 2);

        // Lower z value = closer to camera
        // between 1 and -0.5
        float zValueSpacing = 1.5f / cardsInHand.Count;
        float zStart = 2f;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < cardsInHand.Count; i++) {
            float p = CalculateSplinePosition(firstCardPosition, cardSpacing, i, hoveredIndex);
            float zPos = zStart - (i * zValueSpacing);
            Vector3 splinePosition = spline.EvaluatePosition(p);
            float yPos = splinePosition.y;
            if (cardsInHand[i] == hoveredCard) yPos = splineMiddleYpos;
            Vector3 editedPosition = new Vector3(splinePosition.x, yPos, zPos);
            Vector3 forward = spline.EvaluateTangent(p);
            Debug.Log(forward);
            Vector3 up = spline.EvaluateUpVector(p);
            Debug.Log(up);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);
            Debug.Log(rotation);
            bool isNew = newCards != null && newCards.Contains(cardsInHand[i]);
            MoveSingleCard(cardsInHand[i], editedPosition, rotation, isNew);
    }}

    private float CalculateSplinePosition(float firstPos, float spacing, int index, int hoveredIndex) {
        if (hoveredIndex == -1) {
            return firstPos + (index * spacing);
        }

        // float extraSpacePerSide = 0.05f;
        // 7 - 10
        int temp = Mathf.Clamp(cardsInHand.Count, HAND_START_SCALING, MAX_HAND_SIZE);
        float val = 1 - ((float)(MAX_HAND_SIZE - temp) / ((float)(MAX_HAND_SIZE - HAND_START_SCALING)));
        float extraSpacePerSide = extraDistanceHoverCurve.Evaluate(val);
        float positionCalc;
        float distance = Mathf.Abs(index - hoveredIndex);
        float extraSpacePercent = distanceMovementCurve.Evaluate(distance);
        if (index < hoveredIndex) {
            positionCalc = firstPos + (index * spacing) - extraSpacePercent * extraSpacePerSide;
        } else if (index > hoveredIndex) {
            positionCalc = firstPos + (index * spacing) + extraSpacePercent * extraSpacePerSide;
        } else {
            positionCalc = firstPos + (index * spacing);
        }

        Debug.Log(String.Format("Spline position is : {0}", positionCalc));
        return Mathf.Clamp01(positionCalc);
    }

    private void MoveSingleCard(PlayableCard cardToMove, Vector3 position, Quaternion rotation, bool isNewCard) {
        if (isNewCard) {
            cardToMove.gameObject.SetActive(true);
            Debug.Log(String.Format("PlayerHand: Set cardToMove gameobject active"));
        }
        GameObject moveGO = cardToMove.transform.parent.gameObject;
        GameObject rotateGO = cardToMove.gameObject;
        float rotationTime = 0.25f;
        float moveTime = 0.25f;
        float dealMoveTime = 0.5f;

        if (cardToMove != hoveredCard) {
            LeanTween.rotate(rotateGO, rotation.eulerAngles, rotationTime)
                    .setEase(LeanTweenType.easeInOutQuad);
        } else {
            LeanTween.rotate(rotateGO, Vector3.zero, rotationTime)
                    .setEase(LeanTweenType.easeInOutQuad);
        }

        LeanTween.move(moveGO, position, isNewCard ? dealMoveTime : moveTime)
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

        // Scale from 0 to regular size if dealing card
        if (isNewCard) {
            Vector3 initialScale = moveGO.transform.localScale;
            moveGO.transform.localScale = Vector3.zero;
            LeanTween.value(0f, 1f, moveTime)
                .setOnUpdate((float val) => {
                    moveGO.transform.localScale = val * initialScale;
                });
        }
    }

    private List<PlayableCard> DealCardsQueued(List<Card> cards, DeckInstance deckFrom, bool countAsDraw = true)
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
            cardDealQueue.Enqueue((newCard, countAsDraw));
            // Optionally track which were enqueued
            // (You might want to return instantiated PlayableCards later instead)
            cardsEnqueued.Add(newCard);

            Debug.Log($"[PLAYERHAND.DEAL] Enqueuing card {newCard.card.cardType.GetName()}");
        }

        // Ensure only one coroutine is running
        // if (dealCoroutine == null)
        // {
        //     dealCoroutine = StartCoroutine(ProcessDealQueue());
        // }

        return cardsEnqueued;
    }

    // DealCards deals cards to the hand from the given deck instance.
    // countAsDraw determines whether this counts as drawing a card.
    // Sometimes we generate cards in hand and we do not want that to count towards entity abilities.
    public List<PlayableCard> DealCards(List<Card> cards, DeckInstance deckFrom, bool countAsDraw = true)
    {
        List<PlayableCard> dealt = DealCardsQueued(cards, deckFrom, countAsDraw);
        // If the deck is not null, we'll discard all the cards not successfully dealt to hand.
        if (deckFrom != null)
        {
            var remaining = cards
                .Except(dealt.Select(c => c.card))
                .ToList();
            Debug.Log($"Found {cards.Count} cards that could not be dealt to hand, discarding to deck");
            deckFrom.DiscardCards(remaining);
        }
        return dealt;
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
        // StartCoroutine(card.DiscardToDeck());
        yield return card.DiscardToDeck();
        if (!cardCasted) yield return ResizeHand(card);
        yield return OnCardDiscard(card.deckFrom, card, cardCasted);
    }

    public IEnumerator ResizeHand(PlayableCard card)
    {
        UpdateCardPositions();
        UpdatePlayableCards();
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

    public IEnumerator OnCardDraw(PlayableCard card)
    {
        if (onCardDrawHandler != null)
        {
            foreach (OnCardDrawHandler handler in onCardDrawHandler.GetInvocationList())
            {
                yield return StartCoroutine(handler.Invoke(card));
            }
        }
    }

    public void FocusACard(PlayableCard notThisOne)
    {
        // filter cards by whether they're playable
        List<PlayableCard> playableCards = new List<PlayableCard>();
        foreach (PlayableCard card in cardsInHand)
        {
            if ((notThisOne == null || notThisOne != card) && card.card.cardType.playable)
            {
                playableCards.Add(card);
            }
        }

        // filter cards by whether we have enough mana to play them
        List<PlayableCard> affordableCards = new List<PlayableCard>();
        foreach (PlayableCard card in playableCards)
        {
            if (card.card.GetManaCost() <= ManaManager.Instance.currentMana)
            {
                affordableCards.Add(card);
            }
        }
        if (affordableCards.Count > 0)
        {
            Debug.Log("[NonMouseInputManager] Found a playable and affordable card, hovering card: " + affordableCards[0].name);
            if (affordableCards[0].TryGetComponent<GameObjectFocusable>(out GameObjectFocusable goFocusable))
            {
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
