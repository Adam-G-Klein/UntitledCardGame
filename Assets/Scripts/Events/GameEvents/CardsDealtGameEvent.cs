using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardsDealtEventInfo {
    public List<CardInfo> cards;

    public CardsDealtEventInfo(List<CardInfo> cards){
        this.cards = cards;
    }
}
[CreateAssetMenu(
    fileName = "CardDealtEvent", 
    menuName = "Events/Game Event/Cards Dealt Event")]
public class CardsDealtGameEvent : BaseGameEvent<CardsDealtEventInfo> { }
