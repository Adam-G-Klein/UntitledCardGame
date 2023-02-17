using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffectTargetType {
    // Select some cards from the draw pile, reshuffling to replenish if possible and necessary
    FromDeckWithReshuffle,
    // Select a few cards from the hand
    FromHand,
    // Effect targets entire hand
    EntireHand,
    // effect targets entire deck of targeted companion
    EntireDeck
    //TODO:
    //CompanionDeckWithoutReshuffle,
}

[RequireComponent(typeof(EffectTargetSuppliedEventListener))]
[RequireComponent(typeof(CardSelectionRequestSuppliedEventListener))]
public abstract class TargetProvider: MonoBehaviour {

    protected Entity targetRequestingEntity;
    public TargettableEntity requestedTarget = null;
    protected List<Card> currentCardsToSelectFrom = new List<Card>();
    protected List<Card> currentSelectedCards = new List<Card>();
    protected List<Card> currentUnselectedCards = new List<Card>();

    [SerializeField]
    private EffectTargetRequestEvent effectTargetRequestEvent;
    [SerializeField]
    protected CardSelectionRequestEvent cardSelectionRequestEvent;
    [SerializeField]
    private VoidGameEvent playerHandContentsRequestEvent;
    protected IEnumerator cardTargettingCoroutine;
    protected IEnumerator effectTargettingCoroutine;
    // to prevent us from getting our card selection overridden from someone else (like an effect procedure) requesting the hand contents
    protected bool requestedHandContents;

    void Update() {
    }

    public virtual void effectTargetSuppliedHandler(EffectTargetSuppliedEventInfo eventInfo){
        requestedTarget = eventInfo.target;
        Debug.Log("Effect target supplied handler called, target: " + requestedTarget);
    }

    public virtual void cardSelectionSuppliedHandler(CardSelectionRequestSuppliedEventInfo eventInfo){
        Debug.Log("Card selection supplied handler called");
        Debug.Log("Selected cards: " + eventInfo.selectedCards.Count);
        Debug.Log("Unselected cards: " + eventInfo.unselectedCards.Count);
        this.currentSelectedCards = eventInfo.selectedCards;
        this.currentUnselectedCards = eventInfo.unselectedCards;
    }

    public virtual void playerHandContentsRequestEventHandler(CardListEventInfo eventInfo){
        // This could cause problems
        if(requestedHandContents) {
            this.currentCardsToSelectFrom = eventInfo.cards;
            requestedHandContents = false;
        }
    }
    // DO NOT call this within the class, as it resets the targetting state,
    // and that is definitely not what you're trying to do :)
    // use the coroutine directly instead
    public virtual void requestTarget(List<EntityType> validTargets, TargetRequester requester, List<TargettableEntity> disallowedTargets = null){
        resetTargettingState();
        //StartCoroutine(getTargetCoroutine(validTargets, requester, disallowedTargets));
        this.effectTargettingCoroutine = getTargetCoroutine(validTargets, requester, disallowedTargets);
        StartCoroutine(this.effectTargettingCoroutine);
    }

    // CardsToDraw: the base scale of cards we're asking for, just used when looking at cards from a deck right now
    // SelectedEffect: just for display in the UI by the CardSelectionManager, have to pass this all the way through the request
    public virtual void requestCardTargets(CardEffectTargetType targetType, CardEffect selectedEffect, int cardsToDraw, int minSelections, int maxSelections, TargetRequester requester){
        resetTargettingState();
        this.cardTargettingCoroutine = getCardTargetsCoroutine(targetType, selectedEffect, minSelections, maxSelections, requester, cardsToDraw);
        StartCoroutine(cardTargettingCoroutine);
    }
    private IEnumerator getCardTargetsCoroutine(CardEffectTargetType targetType, CardEffect selectedEffect, int minSelections, int maxSelections, TargetRequester requester, int cardsToDraw = 0) {
        // Might have to do the card selections in here, with different functions for each piece of the switch case
        // just yield returning on their specific coroutines
        switch(targetType) {
            case CardEffectTargetType.FromDeckWithReshuffle:
                yield return StartCoroutine(getCardsFromDeckCoroutine(requester, false, cardsToDraw));
                // prev coroutine updates currentCardsToSelectFrom
                yield return StartCoroutine(cardSelectionRequestEvent.RaiseAtEndOfFrameCoroutine(
                    new CardSelectionRequestEventInfo(currentCardsToSelectFrom, selectedEffect, minSelections, maxSelections)));
                yield return new WaitUntil(() => this.currentSelectedCards.Count > 0 || this.currentUnselectedCards.Count > 0);
                Debug.Log("Card selection request event returned to targets coroutine");
                requester.cardTargetsSupplied(this.currentSelectedCards, this.currentUnselectedCards);
                yield break;
            case CardEffectTargetType.EntireDeck:
                yield return StartCoroutine(getCardsFromDeckCoroutine(requester, true));
                // prev coroutine updates currentCardsToSelectFrom
                // pass that back, as we know the requestor wants everything from the deck
                requester.cardTargetsSupplied(this.currentCardsToSelectFrom, new List<Card>());
                yield break;
            case CardEffectTargetType.FromHand:
                currentCardsToSelectFrom.AddRange(new List<Card>());//TODO playerHand contents request //cardsInHand.ConvertAll(c => c.outOfCombatCard));
                // raise card selection request
                yield break;
            case CardEffectTargetType.EntireHand:
                currentCardsToSelectFrom.AddRange(new List<Card>());//TODO playerHand contents request //cardsInHand.ConvertAll(c => c.outOfCombatCard));
                // just be done once we have the cards in hand in the list
                yield break;
        }
    }

    private IEnumerator getCardsFromDeckCoroutine(TargetRequester requester, bool wholeDeck = false, int cardsToDraw = 0) {
        StartCoroutine(getTargetCoroutine(new List<EntityType>(){EntityType.Companion}, requester));
        yield return new WaitUntil(() => requestedTarget != null);
        CombatEntityWithDeckInstance target = (CombatEntityWithDeckInstance) requestedTarget; 
        if(wholeDeck) {
            cardsToDraw = target.inCombatDeck.totalCards;
        }
        currentCardsToSelectFrom = target.inCombatDeck.dealCardsFromDeck(cardsToDraw, true);
        yield return null;
    }

    private IEnumerator getCardsFromHandCoroutine(CardEffect selectedEffect, int minSelections, int maxSelections, TargetRequester requester) {
        requestedHandContents = true;
        yield return StartCoroutine(playerHandContentsRequestEvent.RaiseAtEndOfFrameCoroutine(null));
        yield return new WaitUntil(() => currentCardsToSelectFrom.Count > 0);
        StartCoroutine(cardSelectionRequestEvent.RaiseAtEndOfFrameCoroutine(
                new CardSelectionRequestEventInfo(currentCardsToSelectFrom, selectedEffect, minSelections, maxSelections)));
        // Waits until the effectTargetSuppliedHandler is called
        yield return new WaitUntil(() => requestedTarget != null);
        requester.effectTargetsSupplied(new List<TargettableEntity>() { requestedTarget });
    }

    protected IEnumerator getTargetCoroutine(List<EntityType> validTargets, TargetRequester requester, List<TargettableEntity> disallowedTargets = null) {
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(
                new EffectTargetRequestEventInfo(validTargets, targetRequestingEntity, disallowedTargets)));
        // Waits until the effectTargetSuppliedHandler is called
        yield return new WaitUntil(() => requestedTarget != null);
        Debug.Log("Target supplied to provider: " + requestedTarget);
        requester.effectTargetsSupplied(new List<TargettableEntity>() { requestedTarget });
    }

    public void resetTargettingState() {
        requestedTarget = null;
        currentSelectedCards.Clear();
        currentUnselectedCards.Clear();
        /*
        if(cardTargettingCoroutine != null)
            StopCoroutine(cardTargettingCoroutine);
        if(effectTargettingCoroutine != null)
            StopCoroutine(effectTargettingCoroutine);
            */
    }

}
