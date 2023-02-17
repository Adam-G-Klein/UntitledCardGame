using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSelectionRequestEventInfo {
    public List<Card> cards;
    // for display in the ui
    public CardEffect selectedEffect;
    public int minSelections;
    public int maxSelections;
    public bool autoSelectAllAvailale = false;

    public CardSelectionRequestEventInfo(List<Card> cards, CardEffect selectedEffect, int minSelections, int maxSelections, bool autoSelectAllAvailale = false) {
        this.cards = cards;
        this.selectedEffect = selectedEffect;
        this.minSelections = minSelections;
        this.maxSelections = maxSelections;
        this.autoSelectAllAvailale = autoSelectAllAvailale;
    }
}
[CreateAssetMenu(
    fileName = "CardSelectionRequestEvent", 
    menuName = "Events/Game Event/CardSelectionRequestEvent")]
public class CardSelectionRequestEvent : BaseGameEvent<CardSelectionRequestEventInfo> { }
