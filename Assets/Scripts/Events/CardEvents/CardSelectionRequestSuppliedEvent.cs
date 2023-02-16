using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSelectionRequestSuppliedEventInfo {
    public List<Card> cards;
    public int minSelections;
    public int maxSelections;
    public bool autoSelectAllAvailale = false;
    public CardEffect selectedAction;
    public CardEffect unselectedAction;

    public CardSelectionRequestSuppliedEventInfo(List<Card> cards, CardEffect selectedAction, CardEffect unselectedAction, int minSelections = 0, int maxSelections = int.MaxValue, bool autoSelectAllAvailale = false) {
        this.cards = cards;
        this.selectedAction = selectedAction;
        this.unselectedAction = unselectedAction;
        this.minSelections = minSelections;
        this.maxSelections = maxSelections;
        this.autoSelectAllAvailale = autoSelectAllAvailale;
    }
}
[CreateAssetMenu(
    fileName = "CardSelectionRequestSuppliedEvent", 
    menuName = "Events/Game Event/CardSelectionRequestSuppliedEvent")]
public class CardSelectionRequestSuppliedEvent : BaseGameEvent<CardSelectionRequestSuppliedEventInfo> { }
