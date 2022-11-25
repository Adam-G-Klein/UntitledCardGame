using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealCardEventInfo {
    public int scale;
    public string target;
    public DealCardEventInfo(int scale, string target){
        this.scale = scale;
        this.target = target;
    }
}

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
public class CardsDealtEvent : BaseGameEvent<CardsDealtEventInfo> { }
