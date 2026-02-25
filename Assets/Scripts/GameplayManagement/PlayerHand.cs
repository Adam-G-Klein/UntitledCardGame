using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using UnityEngine.Playables;
using UnityEditor;
using Unity.VisualScripting;


[ExecuteInEditMode]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class PlayerHand : GenericSingleton<PlayerHand>
{
    // public List<PlayableCard> cardsInHand;
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

    [SerializeField]
    private SplineContainer cardInHandSelectionSpline;


    private GameObject cardPrefab;
    private List<DeckInstance> orderedDeckInstances;
    private Dictionary<DeckInstance, List<PlayableCard>> deckInstanceToPlayableCard;
    private List<PlayableCard> orderedCards;
    public delegate IEnumerator OnCardExhaustHandler(DeckInstance deckFrom, PlayableCard card);
    public PriorityEventDispatcher<OnCardExhaustHandler> onCardExhaustDispatcher = new();

    public delegate IEnumerator OnCardDiscardHandler(DeckInstance deckFrom, PlayableCard card, bool casted);
    public PriorityEventDispatcher<OnCardDiscardHandler> onCardDiscardDispatcher = new();

    public delegate IEnumerator OnCardCastHandler(PlayableCard card);
    public PriorityEventDispatcher<OnCardCastHandler> onCardCastDispatcher = new();

    public delegate IEnumerator OnAttackCardCastHandler(PlayableCard card);
    public PriorityEventDispatcher<OnAttackCardCastHandler> onAttackCardCastDispatcher = new();

    public delegate IEnumerator OnHandEmptyHandler();
    public PriorityEventDispatcher<OnHandEmptyHandler> onHandEmptyDispatcher = new();

    public delegate IEnumerator OnDeckShuffleHandler(DeckInstance deckFrom);
    public PriorityEventDispatcher<OnDeckShuffleHandler> onDeckShuffledDispatcher = new();

    public delegate IEnumerator OnCardDrawHandler(PlayableCard card);
    public PriorityEventDispatcher<OnCardDrawHandler> onCardDrawDispatcher = new();

    private bool cardsInHandLocked = false;
    private readonly float cardDealDelay = .05f;
    private bool canPlayCards = true;
    private bool dealingCardsPreventedEndingTurn = false;

    private float cardBatchingDelay = .1f;
    private int indexToHover = -1;

    private Queue<(PlayableCard, bool)> cardDealQueue = new Queue<(PlayableCard, bool)>();



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
    private int lastHoveredIndex = -1;
    private float splineMiddleYpos;
    public List<PlayableCard> cardsInSelectionSpline = null;

    void Awake()
    {
        this.orderedDeckInstances = new List<DeckInstance>();
        this.deckInstanceToPlayableCard = new Dictionary<DeckInstance, List<PlayableCard>>();
        this.orderedCards = new List<PlayableCard>();
    }

    void Start()
    {
        cardPrefab = EnemyEncounterManager.Instance.encounterConstants.cardPrefab;
        MAX_HAND_SIZE = GameplayConstantsSingleton.Instance.gameplayConstants.MAX_HAND_SIZE;
        splineMiddleYpos = splineContainer.Spline[1].Position.y;

        StartCoroutine(CardDealerWorker());
    }

    // This is called in EnemyEncounterManager after setting up the encounter
    // to ensure the companions have been registered with the combat entity manager
    public void SetupCompanionOrder()
    {
        foreach (CompanionInstance companion in CombatEntityManager.Instance.getCompanions())
        {
            orderedDeckInstances.Add(companion.GetDeckInstance());
            deckInstanceToPlayableCard.Add(companion.GetDeckInstance(), new List<PlayableCard>());
        }
    }

    public void UpdatePlayableCards(DeckInstance deckFrom = null)
    {
        // TODO loop through all cards and update all cards from the related deck
        foreach (PlayableCard playableCard in GetCardsOrdered())
        {
            if (null == deckFrom || playableCard.deckFrom == deckFrom)
            {
                playableCard.UpdateCardText();
            }
        }
        ;
    }

    public List<PlayableCard> GetCardsOrdered()
    {
        return orderedCards;
        // List<PlayableCard> cards = new List<PlayableCard>();
        // foreach (DeckInstance deckInstance in orderedDeckInstances)
        // {
        //     cards.AddRange(deckInstanceToPlayableCard[deckInstance]);
        // }
        // return cards;
    }

    private void UpdateOrderedCards()
    {
        orderedCards.Clear();
        foreach (DeckInstance deckInstance in orderedDeckInstances)
        {
            orderedCards.AddRange(deckInstanceToPlayableCard[deckInstance]);
        }
    }

    private IEnumerator CardDealerWorker()
    {
        while (true)
        {
            // Idle if there are no cards to deal.
            if (cardDealQueue.Count == 0)
            {
                if (dealingCardsPreventedEndingTurn) {
                    EnemyEncounterManager.Instance.CanEndTurn(true);
                    dealingCardsPreventedEndingTurn = false;
                }
                // Bus runs every interval.
                yield return new WaitForSeconds(0.2f);
                continue;
            }

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
                if (newCard.deckFrom == null)
                {
                    Debug.LogError("PlayerHand: Tried to deal a card with a null deck from, this should never happen.");
                    newCards.Remove(x);
                    newCardsWithoutTheBoolField.Remove(x.Item1);
                    continue;
                }
                else if (!orderedDeckInstances.Contains(newCard.deckFrom))
                {
                    Debug.LogError("PlayerHand: The player hand doesn't know about a deck instance that a new card was" +
                        " delt from, adding it to the end of the hand");
                    orderedDeckInstances.Add(newCard.deckFrom);
                    if (!deckInstanceToPlayableCard.ContainsKey(newCard.deckFrom))
                    {
                        deckInstanceToPlayableCard.Add(newCard.deckFrom, new List<PlayableCard>());
                    }
                }

                deckInstanceToPlayableCard[newCard.deckFrom].Add(newCard);

                // Wait to see if we catch any more cards in the batch to let more cards get on the bus.
                yield return new WaitForSeconds(cardBatchingDelay);
            }

            UpdateOrderedCards();
            UpdateCardPositions(newCardsWithoutTheBoolField);

            // process the OnDrawEvents for all the new cards with "countAsDraw" active.
            // Count as draw is needed, because we "draw" cards for example when we generate cards in hand.
            for (int i = 0; i < newCards.Count; i++)
            {
                if (!newCards[i].Item2) continue;
                yield return OnCardDraw(newCards[i].Item1);
                // If the card has an on draw effect workflow, invoke it now.
                CardType ct = newCards[i].Item1.card.cardType;
                if (ct.onDrawEffectWorkflow != null && ct.onDrawEffectWorkflow.effectSteps.Count > 0)
                {
                    List<EffectStep> workflowSteps = ct.onDrawEffectWorkflow.effectSteps;
                    EffectDocument document = new EffectDocument();
                    document.map.AddItem(EffectDocument.ORIGIN, newCards[i].Item1);
                    document.originEntityType = EntityType.Card;
                    Debug.Log("Invoking on draw effect workflow with steps: " + workflowSteps.Count);
                    EffectManager.Instance.invokeEffectWorkflow(document, workflowSteps, null);
                }
            }

            Debug.Log($"[PLAYERHAND.DEAL] Done processing, now updating the playable cards");
            UpdatePlayableCards();
        }
    }

    public void HoverCard(PlayableCard card)
    {
        hoveredCard = card;
        lastHoveredIndex = orderedCards.IndexOf(hoveredCard);
        LeanTween.scale(card.gameObject, new Vector3(hoverScale, hoverScale, 1), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
        float yOffset = hoverYOffset;
        if (card.hoverInPlace) yOffset = 0f;
        LeanTween.moveLocal(card.gameObject, new Vector3(0, yOffset, hoverZOffset), hoverAnimationTime)
            .setEase(LeanTweenType.easeOutQuint);
        UpdateCardPositions(null);

        HighlightRelevantCards(card.deckFrom);
    }

    public void UnhoverCard(PlayableCard card)
    {
        hoveredCard = null;
        LeanTween.scale(card.gameObject, new Vector3(nonHoverScale, nonHoverScale, 1), hoverAnimationTime)
                .setEase(LeanTweenType.easeOutQuint);
        LeanTween.moveLocal(card.gameObject, new Vector3(0, 0, 0), hoverAnimationTime)
                .setEase(LeanTweenType.easeOutQuint);
        UpdateCardPositions();

        HighlightRelevantCards(null);
    }

    public void HighlightRelevantCards(DeckInstance deckFrom)
    {
        foreach (PlayableCard playableCard in GetCardsOrdered())
        {
            // will need to decide which behavior makes sense for the null case
            playableCard.ToggleDarkOverlay(deckFrom != null && deckFrom != playableCard.deckFrom);
        }

        // highlightRelevantCompanions...not 100% sure where this code should live
        CombatEncounterView combatEncounterView = EnemyEncounterManager.Instance.combatEncounterView;
        combatEncounterView.HighlightRelevantCompanions(deckFrom?.combatInstance);
    }

    // Function that updates all card positions
    // Optionally takes a list of new cards to dictate if a card is being newly delt, or just shifting in the hand
    // Optionally takes a playable card, which indicates a card to give extra room in the hand to
    public void UpdateCardPositions(List<PlayableCard> newCards = null)
    {
        if (orderedCards.Count == 0) return;

        float minCardSpacing = 1f / MAX_HAND_SIZE;
        float maxCardSpacing = 1f / HAND_START_SCALING;
        float cardSpacing = Mathf.Clamp(1f / orderedCards.Count, minCardSpacing, maxCardSpacing);
        int hoveredIndex = orderedCards.IndexOf(hoveredCard);
        float firstCardPosition = 0.5f - (orderedCards.Count - 1) * (cardSpacing / 2);

        // Lower z value = closer to camera
        // between 1 and -0.5
        float zValueSpacing = 1.5f / orderedCards.Count;
        float zStart = 2f;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < orderedCards.Count; i++)
        {
            float p = CalculateSplinePosition(firstCardPosition, cardSpacing, i, hoveredIndex, orderedCards.Count);
            float zPos = zStart - (i * zValueSpacing);
            Vector3 splinePosition = spline.EvaluatePosition(p);
            float yPos = splinePosition.y;
            if (orderedCards[i] == hoveredCard) yPos = splineMiddleYpos;
            Vector3 editedPosition = new Vector3(splinePosition.x, yPos, zPos);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);
            bool isNew = newCards != null && newCards.Contains(orderedCards[i]);
            MoveSingleCard(orderedCards[i], editedPosition, rotation, isNew);
        }
    }

    private float CalculateSplinePosition(float firstPos, float spacing, int index, int hoveredIndex, int totalCardsInHand)
    {
        if (hoveredIndex == -1)
        {
            return firstPos + (index * spacing);
        }

        // 7 - 10
        int temp = Mathf.Clamp(totalCardsInHand, HAND_START_SCALING, MAX_HAND_SIZE);
        float val = 1 - ((float)(MAX_HAND_SIZE - temp) / ((float)(MAX_HAND_SIZE - HAND_START_SCALING)));
        float extraSpacePerSide = extraDistanceHoverCurve.Evaluate(val);
        float positionCalc;
        float distance = Mathf.Abs(index - hoveredIndex);
        float extraSpacePercent = distanceMovementCurve.Evaluate(distance);
        if (index < hoveredIndex)
        {
            positionCalc = firstPos + (index * spacing) - extraSpacePercent * extraSpacePerSide;
        }
        else if (index > hoveredIndex)
        {
            positionCalc = firstPos + (index * spacing) + extraSpacePercent * extraSpacePerSide;
        }
        else
        {
            positionCalc = firstPos + (index * spacing);
        }

        Debug.Log(String.Format("Spline position is : {0}", positionCalc));
        return Mathf.Clamp01(positionCalc);
    }

    private void MoveSingleCard(PlayableCard cardToMove, Vector3 position, Quaternion rotation, bool isNewCard)
    {
        if (isNewCard)
        {
            cardToMove.gameObject.SetActive(true);
            Debug.Log(String.Format("PlayerHand: Set cardToMove gameobject active"));
        }
        GameObject moveGO = cardToMove.transform.parent.gameObject;
        GameObject rotateGO = cardToMove.gameObject;
        float rotationTime = 0.25f;
        float moveTime = 0.25f;
        float dealMoveTime = 0.5f;

        if (cardToMove != hoveredCard)
        {
            LeanTween.rotate(rotateGO, rotation.eulerAngles, rotationTime)
                    .setEase(LeanTweenType.easeInOutQuad);
        }
        else
        {
            LeanTween.rotate(rotateGO, Vector3.zero, rotationTime)
                    .setEase(LeanTweenType.easeInOutQuad);
        }

        LeanTween.move(moveGO, position, isNewCard ? dealMoveTime : moveTime)
                .setOnComplete(() =>
                {
                    if (!isNewCard) return;

                    if (cardToMove.TryGetComponent<SpriteRenderer>(out var SR)) SR.sortingLayerName = "Cards"; // what is this magic
                    cardToMove.interactable = true;
                    cardToMove.hoverable = true;

                    if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;

                    List<PlayableCard> cardsInHand = GetCardsOrdered();
                    if (cardsInHand.IndexOf(cardToMove) == indexToHover)
                    {
                        if (cardToMove.TryGetComponent<GameObjectFocusable>(out GameObjectFocusable goFocusable))
                        {
                            FocusManager.Instance.SetFocus(goFocusable);
                        }
                        indexToHover = -1;
                        return;
                    }
                })
                .setEase(LeanTweenType.easeInOutQuad);

        // Scale from 0 to regular size if dealing card
        if (isNewCard)
        {
            Vector3 initialScale = moveGO.transform.localScale;
            moveGO.transform.localScale = Vector3.zero;
            LeanTween.value(0f, 1f, moveTime)
                .setOnUpdate((float val) =>
                {
                    moveGO.transform.localScale = val * initialScale;
                });
        }
    }

    private List<PlayableCard> DealCardsQueued(List<Card> cards, DeckInstance deckFrom, bool countAsDraw = true, bool fromCardCast = false)
    {
        List<PlayableCard> cardsEnqueued = new List<PlayableCard>();
        // If the origin is the card, lets temporarily increase the hand size limit so the card can replace itself.
        int handSizeLimit = GameplayConstantsSingleton.Instance.gameplayConstants.MAX_HAND_SIZE;
        if (fromCardCast)
        {
            handSizeLimit += 1;
        }
        foreach (Card cardInfo in cards)
        {
            if (GetCardsOrdered().Count + cardDealQueue.Count >= handSizeLimit)
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
            newCard.hoverable = false;
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
    public List<PlayableCard> DealCards(List<Card> cards, DeckInstance deckFrom, bool countAsDraw = true, bool fromCardCast = false)
    {
        EnemyEncounterManager.Instance.CanEndTurn(false);
        dealingCardsPreventedEndingTurn = true;
        List<PlayableCard> dealt = DealCardsQueued(cards, deckFrom, countAsDraw, fromCardCast);
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

    public void HoverNextCardAfterCast()
    {
        if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.Mouse) return;

        List<PlayableCard> cardsInHand = GetCardsOrdered();
        if (cardsInHand.Count == 0)
        {
            indexToHover = 0;
            return;
        }
        indexToHover = -1;
        int nextHoverIndex = cardsInHand.Count <= lastHoveredIndex ? lastHoveredIndex - 1 : lastHoveredIndex;

        try
        {
            if (cardsInHand[nextHoverIndex].TryGetComponent<GameObjectFocusable>(out GameObjectFocusable goFocusable))
            {
                FocusManager.Instance.SetFocus(goFocusable);
            }
        } catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void TurnPhaseChangedEventHandler(TurnPhaseEventInfo info)
    {
        if (info.newPhase == TurnPhase.PLAYER_TURN)
        {
            canPlayCards = true;
            indexToHover = 0;
        }
        if (info.newPhase == TurnPhase.END_PLAYER_TURN)
        {
            canPlayCards = false;
            List<PlayableCard> retainedCards = new();
            // Run the effect workflows for all the cards left in the hand,
            // then destroy them with the callback.
            // The reason we want to use the callback is that otherwise, the
            // game objects are destroyed before the effect workflow can complete.
            cardsInHandLocked = true;
            indexToHover = 0;
            List<PlayableCard> cardsInHand = GetCardsOrdered();
            List<PlayableCard> cardsToImmediatelyDiscard = new();
            foreach (PlayableCard card in cardsInHand)
            {
                IEnumerator callback = null;
                // Do not destroy the card if it is retained.
                if (card.retained)
                {
                    retainedCards.Add(card);
                    // If the retain is a temporary effect and not innate to the card,
                    // remove the "retain".
                    if (!card.card.cardType.retain)
                    {
                        card.retained = false;
                    }
                }
                else
                {
                    StartCoroutine(ResizeHand(card));
                    callback = DiscardCard(card, true);
                }
                CardType ct = card.card.cardType;

                if (ct.inPlayerHandEndOfTurnWorkflow != null && ct.inPlayerHandEndOfTurnWorkflow.effectSteps.Count > 0)
                {
                    List<EffectStep> workflowSteps = ct.inPlayerHandEndOfTurnWorkflow.effectSteps;
                    EffectDocument document = new EffectDocument();
                    document.map.AddItem(EffectDocument.ORIGIN, card.GetComponent<PlayableCard>());
                    document.originEntityType = EntityType.Card;
                    Debug.Log("Invoking end of turn effect workflow with steps: " + workflowSteps.Count);
                    EffectManager.Instance.invokeEffectWorkflow(document, workflowSteps, callback);
                }
                else if (!retainedCards.Contains(card))
                {
                    cardsToImmediatelyDiscard.Add(card);
                }
            }
            foreach (PlayableCard card in cardsToImmediatelyDiscard)
            {
                StartCoroutine(DiscardCard(card, true));
            }
            cardsInHandLocked = false;

            cardsInHand = retainedCards;
            UpdateCardPositions();
        }
    }

    public IEnumerator SafeRemoveCardFromHand(PlayableCard card)
    {
        yield return new WaitUntil(() => !cardsInHandLocked);
        // cardsInHand.Remove(card);
        deckInstanceToPlayableCard[card.deckFrom].Remove(card);
        if (cardsInSelectionSpline != null && cardsInSelectionSpline.Contains(card)) cardsInSelectionSpline.Remove(card);
        UpdateOrderedCards();
    }

    public void SafeRemoveCardFromHand(Card card)
    {
        StartCoroutine(SafeRemoveCardFromHand(GetCardById(card.id)));
    }

    public IEnumerator OnCardExhaust(DeckInstance deckFrom, PlayableCard card)
    {
       yield return StartCoroutine(onCardExhaustDispatcher.Invoke(deckFrom, card).GetEnumerator());
    }

    public IEnumerator OnCardDiscard(DeckInstance deckFrom, PlayableCard card, bool casted)
    {
       yield return StartCoroutine(onCardDiscardDispatcher.Invoke(deckFrom, card, casted).GetEnumerator());
    }

    public IEnumerator OnHandEmpty()
    {
       yield return StartCoroutine(onHandEmptyDispatcher.Invoke().GetEnumerator());
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
        if (!cardCasted) EnemyEncounterManager.Instance.combatEncounterState.DiscardCard(card.card);
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
    public IEnumerator ExhaustCard(PlayableCard card)
    {
        // If statements are here to take into account if a card exhausts itself
        // as part of its effect workflow
        if (GetCardsOrdered().Contains(card) || (cardsInSelectionSpline != null && cardsInSelectionSpline.Contains(card)))
        {
            yield return SafeRemoveCardFromHand(card);
        }
        yield return ResizeHand(card);

        if (card.card.cardType.onExhaustEffectWorkflow != null)
        {
            EffectDocument document = new EffectDocument();
            document.originEntityType = EntityType.Card;
            if (card != null) document.map.AddItem<PlayableCard>(EffectDocument.ORIGIN, card);
            EffectManager.Instance.QueueEffectWorkflow(
                new EffectWorkflowClosure(document, card.card.cardType.onExhaustEffectWorkflow, null)
            );
        }
        card.CardExhaustVFX();
        EnemyEncounterManager.Instance.combatEncounterState.ExhaustCard(card.card);
        yield return OnCardExhaust(card.deckFrom, card);
        // Queue up the callback last before returning execution so the callback will destroy the card
        // ONLY after the on exhaust workflows.
        // Any workflows queued by `deckFrom.ExhaustCard` will run BEFORE this.
        if (card.gameObject.activeSelf)
        {
            EffectManager.Instance.QueueEffectWorkflow(
                new EffectWorkflowClosure(new EffectDocument(), new EffectWorkflow(), card.ExhaustCard())
            );
        }
    }

    public PlayableCard GetCardById(string id)
    {
        foreach (PlayableCard card in GetCardsOrdered())
        {
            if (card.card.id == id)
            {
                return card;
            }
        }
        Debug.LogWarning("PlayerHand: Unable to find card in hand with id " + id);
        return null;
    }

    public IEnumerator OnCardCast(PlayableCard card)
    {
        yield return StartCoroutine(onCardCastDispatcher.Invoke(card).GetEnumerator());
        if (card.card.cardType.cardCategory == CardCategory.Attack)
        {
            yield return StartCoroutine(onAttackCardCastDispatcher.Invoke(card).GetEnumerator());
        }
    }

    public IEnumerator OnDeckShuffled(DeckInstance deck)
    {
        yield return StartCoroutine(onDeckShuffledDispatcher.Invoke(deck).GetEnumerator());
    }

    public IEnumerator OnCardDraw(PlayableCard card)
    {
        yield return StartCoroutine(onCardDrawDispatcher.Invoke(card).GetEnumerator());
    }

    public void FocusACard(PlayableCard notThisOne)
    {
        // filter cards by whether they're playable
        List<PlayableCard> playableCards = new List<PlayableCard>();
        foreach (PlayableCard card in GetCardsOrdered())
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

    public void DisableHand()
    {
        canPlayCards = false;
        foreach (PlayableCard card in GetCardsOrdered())
        {
            card.interactable = false; // not sure why this wasn't enough honestly :/
        }
    }

    public void SetHoverable(bool boolean)
    {
        foreach (PlayableCard card in GetCardsOrdered())
        {
            card.hoverable = boolean;
        }
    }

    public bool GetCanPlayCards()
    {
        return canPlayCards;
    }

    public void SelectCardsFromHand(
            int number,
            List<GameObject> disallowedCards,
            List<GameObject> cardsLimitedTo,
            Action<CancelContext> cancelCallback,
            Action<List<PlayableCard>> callback,
            PlayableCard cardCast, // Nullable
            bool canCancel,
            string helperText = "")
    {
        string CHOOSE_X_CARDS = "Choose {0} cards{1}";
        string CHOOSE_A_CARD = "Choose a card{0}";
        string SELECT_CONFIRM = "Select Confirm";

        CombatEncounterView combatEncounterView = EnemyEncounterManager.Instance.combatEncounterView;
        CardInHandSelectionView selectionView = combatEncounterView.GetCardSelectionView();

        SetHoverable(true);

        List<PlayableCard> selectedCards = new List<PlayableCard>();

        TargettingManager.Instance.targetSuppliedHandler += SelectingCardSuppliedHandler;
        TargettingManager.Instance.cancelTargettingHandler += SelectingCardCancelHandler;

        combatEncounterView.SetCompanionsAndEnemiesEnabled(false);
        combatEncounterView.DestroyAllTooltips();
        selectionView.SetConfirmedHandler(SelectingCardConfirmed);
        if (canCancel) selectionView.EnableCancelHandler(() => UIStateManager.Instance.TryCancelTargetting());
        selectionView.EnableSelection(GetPromptText(), GeoChangedHandler, canCancel);

        void SelectingCardSuppliedHandler(Targetable target)
        {
            // Check if the provided target is disallowed
            if (disallowedCards != null && disallowedCards.Contains(target.gameObject))
                return;

            // Check if we only want to pick from a specific list of targets
            if (cardsLimitedTo != null && !cardsLimitedTo.Contains(target.gameObject))
            {
                return;
            }

            // Check if the entity type is correct
            if (target.targetType != Targetable.TargetType.Card)
            {
                return;
            }

            if (target.TryGetComponent<PlayableCard>(out PlayableCard targetCard))
            {
                PlayableCard nextCardToHover = null;
                if (selectedCards.Contains(targetCard))
                {
                    // Move from selected spline back to player hand
                    selectedCards.Remove(targetCard);
                    deckInstanceToPlayableCard[targetCard.deckFrom].Add(targetCard);
                    UpdateOrderedCards();
                    targetCard.hoverInPlace = false;
                    nextCardToHover = targetCard;
                }
                else if (selectedCards.Count != number)
                {
                    // Move from hand to selected spline
                    deckInstanceToPlayableCard[targetCard.deckFrom].Remove(targetCard);
                    selectedCards.Add(targetCard);
                    targetCard.hoverInPlace = true;

                    // In a complicated fashion, find the next card to hover
                    int indexOfTargetCardBeforeRemoving = orderedCards.IndexOf(targetCard);
                    int nextIndex = 0;
                    if (orderedCards.Count == 1)
                    {
                        nextIndex = -1;
                    }
                    else if (indexOfTargetCardBeforeRemoving == orderedCards.Count - 1)
                    {
                        nextIndex = indexOfTargetCardBeforeRemoving - 1;
                    }
                    else if (indexOfTargetCardBeforeRemoving == 0)
                    {
                        nextIndex = 1;
                    }
                    else
                    {
                        nextIndex = indexOfTargetCardBeforeRemoving + 1;
                    }

                    if (nextIndex != -1)
                    {
                        nextCardToHover = orderedCards[nextIndex];
                    }

                }
                else
                {
                    return;
                }
                UpdateOrderedCards();
                // targetCard.interactable = false;
                UnhoverCard(targetCard);
                UpdateCardSelectionSplineCardPositions();
                UpdateCardPositions();
                selectionView.UpdateLabelText(GetPromptText());

                if (selectedCards.Count == number && ControlsManager.Instance.GetControlMethod() != ControlsManager.ControlMethod.Mouse) {
                    StartCoroutine(DoAfterAllTweens(() => FocusManager.Instance.SetFocus(selectionView.confirmButton.AsFocusable())));
                }
                else if (nextCardToHover != null && ControlsManager.Instance.GetControlMethod() != ControlsManager.ControlMethod.Mouse)
                {
                    StartCoroutine(DoAfterAllTweens(() => FocusManager.Instance.SetFocus(nextCardToHover.GetComponent<GameObjectFocusable>())));
                }
            }
        }

        void SelectingCardCancelHandler(CancelContext context)
        {
            cancelCallback(context);
            if (context.canCancel != true) return;
            combatEncounterView.SetCompanionsAndEnemiesEnabled(true);
            selectionView.RemoveConfirmedHandler(SelectingCardConfirmed);
            selectionView.RemoveCancelHandler();
            TargettingManager.Instance.targetSuppliedHandler -= SelectingCardSuppliedHandler;
            TargettingManager.Instance.cancelTargettingHandler -= SelectingCardCancelHandler;
            selectionView.DisableSelection();
            List<PlayableCard> temp = new List<PlayableCard>(selectedCards);
            foreach (PlayableCard card in temp)
            {
                selectedCards.Remove(card);
                deckInstanceToPlayableCard[card.deckFrom].Add(card);
            }
            if (cardCast != null) {
                cardCast.hoverable = true;
                deckInstanceToPlayableCard[cardCast.deckFrom].Add(cardCast);
            }
            UpdateOrderedCards();
            UpdateCardPositions();
            cardsInSelectionSpline = null;
        }

        void SelectingCardConfirmed()
        {
            if (selectedCards.Count != number) return;
            combatEncounterView.SetCompanionsAndEnemiesEnabled(true);
            selectionView.RemoveConfirmedHandler(SelectingCardConfirmed);
            selectionView.RemoveCancelHandler();
            TargettingManager.Instance.targetSuppliedHandler -= SelectingCardSuppliedHandler;
            TargettingManager.Instance.cancelTargettingHandler -= SelectingCardCancelHandler;
            selectionView.DisableSelection();
            cardsInSelectionSpline = new List<PlayableCard>(selectedCards);
            callback(selectedCards);
            EffectManager.Instance.RegisterEffectWorkflowFinishedDelegate(() =>
            {
                List<PlayableCard> temp = new List<PlayableCard>(cardsInSelectionSpline);
                foreach (PlayableCard card in temp)
                {
                    Debug.Log(String.Format("SelectingCardConfirmed: {0}", card));
                    deckInstanceToPlayableCard[card.deckFrom].Add(card);
                    cardsInSelectionSpline.Remove(card);
                    card.hoverInPlace = false;
                }
                UpdateOrderedCards();
                UpdateCardPositions();
            });
        }

        string GetPromptText()
        {
            int cardsRemainingToSelect = number - selectedCards.Count;
            if (cardsRemainingToSelect > 1)
            {
                return String.Format(CHOOSE_X_CARDS, cardsRemainingToSelect, helperText);
            }
            else if (cardsRemainingToSelect == 1)
            {
                return String.Format(CHOOSE_A_CARD, helperText);
            }

            return SELECT_CONFIRM;
        }

        void GeoChangedHandler(GeometryChangedEvent evt)
        {
            // Update the position of the spline
            BezierKnot startPoint = cardInHandSelectionSpline.Spline[0];
            startPoint.Position = selectionView.GetSplineStartpoint();
            cardInHandSelectionSpline.Spline.SetKnot(0, startPoint);
            BezierKnot endPoint = cardInHandSelectionSpline.Spline[1];
            endPoint.Position = selectionView.GetSplineEndpoint();
            cardInHandSelectionSpline.Spline.SetKnot(1, endPoint);
            if (cardCast != null)
            {
                UnhoverCard(cardCast);
                cardCast.hoverable = false;
                Vector3 worldspacePosition = UIDocumentGameObjectPlacer.GetWorldPositionFromElement(selectionView.GetCardCastLocationElement());
                Debug.Log("SelectCardsFromHand: worldspace position " + worldspacePosition.ToString());
                deckInstanceToPlayableCard[cardCast.deckFrom].Remove(cardCast);
                UpdateOrderedCards();
                UpdateCardPositions();
                MoveSingleCard(cardCast, worldspacePosition, Quaternion.identity, false);
            }
        }

        void UpdateCardSelectionSplineCardPositions()
        {
            float minCardSpacing = 1f / MAX_HAND_SIZE;
            float maxCardSpacing = 1f / 4;
            float cardSpacing = Mathf.Clamp(1f / selectedCards.Count, minCardSpacing, maxCardSpacing);
            float firstCardPosition = 0.5f - (selectedCards.Count - 1) * (cardSpacing / 2);

            // Lower z value = closer to camera
            // between 1 and -0.5
            float zValueSpacing = 1.5f / selectedCards.Count;
            float zStart = 2f;
            Spline spline = cardInHandSelectionSpline.Spline;

            for (int i = 0; i < selectedCards.Count; i++)
            {
                float p = firstCardPosition + (i * cardSpacing);
                float zPos = zStart - (i * zValueSpacing);
                Vector3 splinePosition = spline.EvaluatePosition(p);
                float yPos = splinePosition.y;
                Vector3 editedPosition = new Vector3(splinePosition.x, yPos, zPos);
                MoveSingleCard(selectedCards[i], editedPosition, Quaternion.identity, false);
            }
        }
    }

    private IEnumerator DoAfterAllTweens(Action action)
    {
        yield return new WaitUntil(() => LeanTween.tweensRunning == 0);
        action?.Invoke();
    }
}
