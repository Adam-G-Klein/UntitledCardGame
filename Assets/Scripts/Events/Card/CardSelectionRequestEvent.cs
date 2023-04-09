using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSelectionRequestEventInfo {
    public List<Card> cards;
    public int minSelections;
    public int maxSelections;
    public CardEffect selectedAction;
    public CardEffect unselectedAction;

    public CardSelectionRequestEventInfo(List<Card> cards, CardEffect selectedAction, CardEffect unselectedAction, int minSelections = 0, int maxSelections = int.MaxValue) {
        this.cards = cards;
        this.selectedAction = selectedAction;
        this.unselectedAction = unselectedAction;
        this.minSelections = minSelections;
        this.maxSelections = maxSelections;
    }
}

[CreateAssetMenu(
    fileName = "NewCardSelectionRequestEvent", 
    menuName = "Events/Card/Card Selection Request Event")]
public class CardSelectionRequestEvent : BaseGameEvent<CardSelectionRequestEventInfo> { }
