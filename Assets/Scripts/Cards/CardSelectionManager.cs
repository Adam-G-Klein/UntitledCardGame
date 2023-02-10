using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Okay so current idea is that the selected cards will 
// have one action associated with them, and the unselected cards
// will have a different one (add to hand, discard)

[RequireComponent(typeof(CardListEventListener))]
[RequireComponent(typeof(CardSelectionRequestEventListener))]
public class CardSelectionManager: MonoBehaviour{
    public GameObject cardSelectionUIPrefab;
    [SerializeField]
    private EffectTargetRequestEvent effectTargetRequestEvent;
    [SerializeField]
    private BoolGameEvent cardSelectionValidEvent;

    private IEnumerator selectionCoroutine;
    private List<Card> selectedCards = new List<Card>();
    private List<Card> unselectedCards = new List<Card>();
    private bool selectionConfirmed = false;
    private int currentMinSelection = 0;
    private CardSelectionAction currentSelectedAction;
    private CardSelectionAction currentUnselectedAction;

    public void cardSelectedEventHandler(CardListEventInfo info){
        selectedCards.AddRange(info.cards);
        unselectedCards.RemoveAll(c => info.cards.Contains(c));
        StartCoroutine(cardSelectionValidEvent.RaiseAtEndOfFrameCoroutine(selectedCards.Count > currentMinSelection));
    }

    public void cardDeselectedEventHandler(CardListEventInfo info){
        unselectedCards.AddRange(info.cards);
        selectedCards.RemoveAll(c => info.cards.Contains(c));
        StartCoroutine(cardSelectionValidEvent.RaiseAtEndOfFrameCoroutine(selectedCards.Count > currentMinSelection));
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

    public void applyCardAction(Card card, CardSelectionAction action) {
        switch(action) {
            case CardSelectionAction.ADD_TO_HAND:
                Debug.Log("Adding card " + card.id + " to hand");
                break;
            case CardSelectionAction.DISCARD:
                Debug.Log("Discarding card " + card.id); 
                break;
            case CardSelectionAction.EXHAUST:
                Debug.Log("Exhausting card " + card.id);
                break;
            case CardSelectionAction.PURGE:
                Debug.Log("Purging card " + card.id);
                break;
        }
    }
    public void cardSelectionRequestHandler(CardSelectionRequestEventInfo info) {
        resetSelectionState();
        unselectedCards.AddRange(info.cards);
        this.currentMinSelection = info.minSelections;
        this.currentSelectedAction = info.selectedAction;
        this.currentUnselectedAction = info.unselectedAction;
        displayCardGroup(info.cards);
        // using this event allows us to reuse the logic in TargettableEntity in UICard
        // even though we're not using the TargetSupplied event to pass them back
        StartCoroutine(effectTargetRequestEvent.RaiseAtEndOfFrameCoroutine(new EffectTargetRequestEventInfo(new List<EntityType>(){EntityType.UICard})));
    }

    private void displayCardGroup(List<Card> cards) {
        GameObject cardSelectionUI = Instantiate(cardSelectionUIPrefab);
        CardViewUI cardViewUI = cardSelectionUI.GetComponent<CardViewUI>();
        cardViewUI.Setup(cards);
    }

    private void resetSelectionState() {
        selectionConfirmed = false;
        selectedCards.Clear();
        unselectedCards.Clear();
        this.currentMinSelection = 0;
        this.currentSelectedAction = CardSelectionAction.DISCARD;
        this.currentUnselectedAction = CardSelectionAction.DISCARD;
    }


}