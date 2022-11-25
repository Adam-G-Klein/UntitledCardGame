using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
