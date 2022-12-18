using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The cast event is what actually resets the UI, removes the card 
// from the player hand, charges mana, kicks off animations
[System.Serializable]
public class CardCastEventInfo {
    public CardInfo cardInfo;

    public CardCastEventInfo(CardInfo cardInfo) {
        this.cardInfo = cardInfo;
    }
}
[CreateAssetMenu(
    fileName = "CardCastEvent", 
    menuName = "Events/Game Event/Card Cast Event")]
public class CardCastEvent : BaseGameEvent<CardCastEventInfo> { }
