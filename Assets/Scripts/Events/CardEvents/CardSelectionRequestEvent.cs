using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardSelectionAction {
    ADD_TO_HAND,
    DISCARD,
    // remove from in combat deck
    EXHAUST,
    // remove from deck for rest of the run
    PURGE,
}

[System.Serializable]
public class CardSelectionRequestEventInfo {
    public List<Card> cards;
    public int minSelections;
    public CardSelectionAction selectedAction;
    public CardSelectionAction unselectedAction;

    public CardSelectionRequestEventInfo(List<Card> cards, CardSelectionAction selectedAction, CardSelectionAction unselectedAction, int minSelections = 0) {
        this.cards = cards;
        this.selectedAction = selectedAction;
        this.unselectedAction = unselectedAction;
        this.minSelections = minSelections;
    }
}
[CreateAssetMenu(
    fileName = "CardSelectionRequestEvent", 
    menuName = "Events/Game Event/CardSelectionRequestEvent")]
public class CardSelectionRequestEvent : BaseGameEvent<CardSelectionRequestEventInfo> { }
