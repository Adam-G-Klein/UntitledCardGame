using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffectEventInfo {
    public CardEffectName effectName;
    public int scale;
    public string target;

    // TODO: multiple targets allowed
    public CardEffectEventInfo(CardEffectName effectName, int scale, string target){
        this.effectName = effectName;
        this.scale = scale;
        this.target = target;
    }
}
[CreateAssetMenu(
    fileName = "CardEffectEvent", 
    menuName = "Events/Game Event/Card Effect Event")]
public class CardEffectEvent : BaseGameEvent<CardEffectEventInfo> { }
