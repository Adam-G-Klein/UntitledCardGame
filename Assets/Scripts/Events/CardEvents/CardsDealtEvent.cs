using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardsDealtEventInfo {
    public List<Card> cards;
    public CombatEntityInEncounterStats fromStats;
    public InCombatDeck fromDeck;

    public CardsDealtEventInfo(List<Card> cards, InCombatDeck fromDeck, CombatEntityInEncounterStats fromStats) {
        this.cards = cards;
        this.fromStats = fromStats;
        this.fromDeck = fromDeck;
    }
}
[CreateAssetMenu(
    fileName = "CardDealtEvent", 
    menuName = "Events/Game Event/Cards Dealt Event")]
public class CardsDealtEvent : BaseGameEvent<CardsDealtEventInfo> { }
