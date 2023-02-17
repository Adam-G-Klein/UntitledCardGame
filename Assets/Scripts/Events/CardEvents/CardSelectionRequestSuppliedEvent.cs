using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSelectionRequestSuppliedEventInfo {
    public List<Card> selectedCards;
    public List<Card> unselectedCards;

    public CardSelectionRequestSuppliedEventInfo(List<Card> selectedCards, List<Card> unselectedCards) {
        this.selectedCards = selectedCards;
        this.unselectedCards = unselectedCards;
    }

}
[CreateAssetMenu(
    fileName = "CardSelectionRequestSuppliedEvent", 
    menuName = "Events/Game Event/CardSelectionRequestSuppliedEvent")]
public class CardSelectionRequestSuppliedEvent : BaseGameEvent<CardSelectionRequestSuppliedEventInfo> { }
