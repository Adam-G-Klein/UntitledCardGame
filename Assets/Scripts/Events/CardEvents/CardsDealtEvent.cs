using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardsDealtEventInfo {
    public List<CardInfo> cards;
    public Companion companionFrom;

    public CardsDealtEventInfo(List<CardInfo> cards, Companion companionFrom) {
        this.cards = cards;
        this.companionFrom = companionFrom;
    }
}
[CreateAssetMenu(
    fileName = "CardDealtEvent", 
    menuName = "Events/Game Event/Cards Dealt Event")]
public class CardsDealtEvent : BaseGameEvent<CardsDealtEventInfo> { }
