using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Okay so current idea is that the selected cards will 
// have one action associated with them, and the unselected cards
// will have a different one (add to hand, discard)

[RequireComponent(typeof(CardListEventListener))]
[RequireComponent(typeof(CardSelectionRequestEventListener))]
public class CardSelectionManager: GenericSingleton<CardSelectionManager> {
    public GameObject cardSelectionUIPrefab;
    [SerializeField]
    private EffectTargetRequestEvent effectTargetRequestEvent;
    [SerializeField]
    private BoolGameEvent cardSelectionValidEvent;
    [SerializeField]
    private CardListEvent deselectCardsEvent;
    [SerializeField]
    private CardEffectEvent cardEffectEvent;

    private IEnumerator selectionCoroutine;
    private List<Card> selectedCards = new List<Card>();
    private List<Card> unselectedCards = new List<Card>();
    private int currentMinSelection = 0;
    private int currentMaxSelection = 0;
    private CardEffectType currentSelectedAction;
    private CardEffectType currentUnselectedAction;
    

    public void cardSelectedEventHandler(CardListEventInfo info){
        selectedCards.AddRange(info.cards);
        unselectedCards.RemoveAll(c => info.cards.Contains(c));
        if(selectedCards.Count > currentMaxSelection) {
            StartCoroutine(deselectCardsEvent.RaiseAtEndOfFrameCoroutine(new CardListEventInfo(new List<Card>(){selectedCards[0]})));
            selectedCards.RemoveAt(0);
        }
        StartCoroutine(cardSelectionValidEvent.RaiseAtEndOfFrameCoroutine(selectedCards.Count >= currentMinSelection && selectedCards.Count <= currentMaxSelection));
    }

    public void cardDeselectedEventHandler(CardListEventInfo info){
        unselectedCards.AddRange(info.cards);
        selectedCards.RemoveAll(c => info.cards.Contains(c));
        StartCoroutine(cardSelectionValidEvent.RaiseAtEndOfFrameCoroutine(selectedCards.Count >= currentMinSelection && selectedCards.Count <= currentMaxSelection));
    }

    public void UiCardsSelectionConfirmed() {
        Debug.Log("Selection confirmed");
        foreach(Card card in selectedCards) {
            applyCardAction(card, this.currentSelectedAction);
        }
        foreach(Card card in unselectedCards) {
            applyCardAction(card, this.currentUnselectedAction);
        }
        resetSelectionState();
    }

    public void applyCardAction(Card card, CardEffectType action) {
        // this function might get more complicated in the future if 
        // any of the actions aren't just handled by the listeners on 
        // the entity that owns the card
        // See CombatEntityWithDeckInstance.cs
        raiseCardEffect(card, action);
    }

    private void raiseCardEffect(Card card, CardEffectType action) {
        StartCoroutine(cardEffectEvent.RaiseAtEndOfFrameCoroutine(
            new CardEffectEventInfo(
                new Dictionary<CardEffectType, int>(){ {action, 1} }, 
                null, 
                new List<Card>(){card})));
    }

    public void cardSelectionRequestHandler(CardSelectionRequestEventInfo info) {
        resetSelectionState();
        unselectedCards.AddRange(info.cards);
        this.currentMinSelection = Mathf.Min(info.minSelections, info.cards.Count);
        this.currentMaxSelection = info.maxSelections;
        this.currentSelectedAction = info.selectedAction;
        this.currentUnselectedAction = info.unselectedAction;
        displayCardGroup(info.cards, info.selectedAction, info.minSelections);
        // using this event allows us to reuse the logic in TargettableEntity in UICard
        // even though we're not using the TargetSupplied event to pass them back
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(new EffectTargetRequestEventInfo(new List<EntityType>(){EntityType.UICard})));
    }

    private void displayCardGroup(List<Card> cards, CardEffectType selectedAction, int minSelections) {
        GameObject cardSelectionUI = Instantiate(cardSelectionUIPrefab);
        CardViewUI cardViewUI = cardSelectionUI.GetComponent<CardViewUI>();
        TextMeshProUGUI prompt = cardSelectionUI.GetComponentInChildren<TextMeshProUGUI>();
        prompt.text = getCurrentPromptText();
        cardViewUI.Setup(cards);
    }

    private string getCurrentPromptText() {
        if(currentMinSelection > 0 && currentMaxSelection != int.MaxValue) {
            return "Select " + currentMinSelection + " card" + (currentMinSelection > 1 ? "s" : "") + " to " + currentSelectedAction.GetDescription();
        }
        if(currentMinSelection > 0) {
            return "Select at least " + currentMinSelection  + " card" + (currentMinSelection > 1 ? "s" : "") + " to " + currentSelectedAction.GetDescription();
        }
        if(currentMaxSelection != int.MaxValue) {
            return "Select up to " + currentMaxSelection + " card" + (currentMaxSelection > 1 ? "s" : "") + " to " + currentSelectedAction.GetDescription();
        }
        return "Select cards to " + currentSelectedAction.ToString();
    }

    private void resetSelectionState() {
        selectedCards.Clear();
        unselectedCards.Clear();
        this.currentMinSelection = 0;
        this.currentMaxSelection = int.MaxValue;
        this.currentSelectedAction = CardEffectType.Discard;
        this.currentUnselectedAction = CardEffectType.Discard;
    }


}