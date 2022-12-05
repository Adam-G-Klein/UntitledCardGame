using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardsDealtEventInfo {
    public List<CardInfo> cards;
    public CombatEntityInEncounterStats companionFromStats;

    public CardsDealtEventInfo(List<CardInfo> cards, CombatEntityInEncounterStats companionFromStats) {
        this.cards = cards;
        this.companionFromStats = companionFromStats;
    }
}
[CreateAssetMenu(
    fileName = "CardDealtEvent", 
    menuName = "Events/Game Event/Cards Dealt Event")]
public class CardsDealtEvent : BaseGameEvent<CardsDealtEventInfo> { }
