using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardsDealtEventInfo {
    public List<Card> cards;
    public CombatEntityWithDeckInstance entityFrom;
    public CombatEntityInEncounterStats fromStats;

    public CardsDealtEventInfo(List<Card> cards, CombatEntityWithDeckInstance entityFrom) {
        this.cards = cards;
        this.entityFrom = entityFrom;
        this.fromStats = entityFrom.stats;
    }
}
[CreateAssetMenu(
    fileName = "CardDealtEvent", 
    menuName = "Events/Game Event/Cards Dealt Event")]
public class CardsDealtEvent : BaseGameEvent<CardsDealtEventInfo> { }
