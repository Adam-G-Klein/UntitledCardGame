using System.Collections.Generic;
using UnityEngine;

// The cast event is what actually resets the UI, removes the card 
// from the player hand, charges mana, kicks off animations
[System.Serializable]
public class CardListEventInfo {
    public List<Card> cards;

    public CardListEventInfo(List<Card> cards) {
        this.cards = cards;
    }
}

[CreateAssetMenu(
    fileName = "NewCardCastEvent", 
    menuName = "Events/Card/Card List Event")]
public class CardListEvent : BaseGameEvent<CardListEventInfo> { }