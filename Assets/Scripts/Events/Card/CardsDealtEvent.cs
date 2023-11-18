using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardsDealtEventInfo {
    public List<Card> cards;
    public DeckInstance deckFrom;
    public CombatInstance combatInstance;

    public CardsDealtEventInfo(List<Card> cards, DeckInstance deckFrom) {
        this.cards = cards;
        this.deckFrom = deckFrom;
        this.combatInstance = deckFrom.combatInstance;
    }
}

[CreateAssetMenu(
    fileName = "NewCardDealtEvent", 
    menuName = "Events/Card/Cards Dealt Event")]
public class CardsDealtEvent : BaseGameEvent<CardsDealtEventInfo> { }