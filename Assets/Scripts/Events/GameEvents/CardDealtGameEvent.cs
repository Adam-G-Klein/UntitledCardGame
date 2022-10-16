using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDealtEventInfo {
    public List<CardInfo> cards;

    public CardDealtEventInfo(List<CardInfo> cards){
        this.cards = cards;
    }
}
[CreateAssetMenu(
    fileName = "CardDealtEvent", 
    menuName = "Events/Game Event/CardDealt Event")]
public class CardDealtGameEvent : BaseGameEvent<CardDealtEventInfo> { }
