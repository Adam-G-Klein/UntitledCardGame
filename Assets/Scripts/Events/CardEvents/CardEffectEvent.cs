using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffect {
    // Anything else that can be done to cards will go here
    Discard
}
// An event that announces any effect that targets a card
[System.Serializable]
public class CardEffectEventInfo {
    public Dictionary<CardEffect, int> cardEffects;
    public List<TargettableEntity> targets;

    public CardEffectEventInfo(Dictionary<CardEffect, int> cardEffects, List<TargettableEntity> targets) {
        this.cardEffects = cardEffects;
        this.targets = targets;
    }

}
[CreateAssetMenu(
    fileName = "CardEffectEvent", 
    menuName = "Events/Game Event/Card Effect Event")]
public class CardEffectEvent : BaseGameEvent<CardEffectEventInfo> { }
