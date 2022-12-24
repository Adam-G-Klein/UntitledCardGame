using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[System.Serializable]
public class EffectTargetSuppliedEventInfo {
    public TargettableEntity target;

    public EffectTargetSuppliedEventInfo(TargettableEntity target){
        this.target = target;
    }
}

[CreateAssetMenu(
    fileName = "EffectTargetSuppliedEvent",
    menuName = "Events/Game Event/Effect Target Supplied Event")]
public class EffectTargetSuppliedEvent : BaseGameEvent<EffectTargetSuppliedEventInfo> { }
