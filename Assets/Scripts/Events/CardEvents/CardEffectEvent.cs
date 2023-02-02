using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffectEventInfo {
    public SimpleEffectName effectName;
    public int scale;
    public Dictionary<StatusEffect, int> statusEffects;
    public List<TargettableEntity> targets;

    public CardEffectEventInfo(SimpleEffectName effectName, int scale, List<TargettableEntity> targets, Dictionary<StatusEffect, int> statusEffects){
        this.effectName = effectName;
        this.scale = scale;
        this.targets = targets;
        this.statusEffects = statusEffects;
    }
}
[CreateAssetMenu(
    fileName = "CardEffectEvent", 
    menuName = "Events/Game Event/Card Effect Event")]
public class CardEffectEvent : BaseGameEvent<CardEffectEventInfo> { }
