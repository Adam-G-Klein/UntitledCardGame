using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum CardEffectType {
    // Anything else that can be done to cards will go here
    Discard,
    // Uses the enumUtils to generate a nice description when displaying the action
    // in a selection menu
    [Description("Add To Hand")]
    AddToHand,
    Exhaust,
    Purge
}

// An event that announces any effect that targets a card
[System.Serializable]
public class CardEffectEventInfo {
    // in case we want to add a buff effect in the future
    public Dictionary<CardEffectType, int> cardEffects;
    public List<Card> cards;
    public List<TargettableEntity> targets;

    public CardEffectEventInfo(Dictionary<CardEffectType, int> cardEffects, List<TargettableEntity> targets = null, List<Card> cards = null) {
        this.cardEffects = cardEffects;
        this.targets = targets;
        this.cards = cards;
        if(this.cards == null) {
            this.cards = new List<Card>();
        }
        if(this.targets == null) {
            this.targets = new List<TargettableEntity>();
        }
    }
}

[CreateAssetMenu(
    fileName = "NewCardEffectEvent", 
    menuName = "Events/Card/Card Effect Event")]
public class CardEffectEvent : BaseGameEvent<CardEffectEventInfo> { }
