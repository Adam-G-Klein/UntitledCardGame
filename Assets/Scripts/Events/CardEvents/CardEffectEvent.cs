using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffectEventInfo {
    public CardEffectName effectName;
    public int scale;
    public List<string> targets;

    public CardEffectEventInfo(CardEffectName effectName, int scale, List<string> targetIds){
        this.effectName = effectName;
        this.scale = scale;
        this.targets = targetIds;
    }
}
[CreateAssetMenu(
    fileName = "CardEffectEvent", 
    menuName = "Events/Game Event/Card Effect Event")]
public class CardEffectEvent : BaseGameEvent<CardEffectEventInfo> { }
