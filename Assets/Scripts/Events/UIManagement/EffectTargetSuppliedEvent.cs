using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[System.Serializable]
public class EffectTargetSuppliedEventInfo {
    public CardEffectData effect;
    public CombatEntityInstance target;

    public EffectTargetSuppliedEventInfo(CardEffectData effect, CombatEntityInstance target){
        this.effect = effect;
        this.target = target;
    }
}

[CreateAssetMenu(
    fileName = "EffectTargetSuppliedEvent",
    menuName = "Events/Game Event/Effect Target Supplied Event")]
public class EffectTargetSuppliedEvent : BaseGameEvent<EffectTargetSuppliedEventInfo> { }
