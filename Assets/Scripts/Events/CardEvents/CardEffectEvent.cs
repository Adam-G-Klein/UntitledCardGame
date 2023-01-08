using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffectEventInfo {
    public SimpleEffectName effectName;
    public int scale;
    public List<TargettableEntity> targets;

    public CardEffectEventInfo(SimpleEffectName effectName, int scale, List<TargettableEntity> targets){
        this.effectName = effectName;
        this.scale = scale;
        this.targets = targets;
    }
}
[CreateAssetMenu(
    fileName = "CardEffectEvent", 
    menuName = "Events/Game Event/Card Effect Event")]
public class CardEffectEvent : BaseGameEvent<CardEffectEventInfo> { }
