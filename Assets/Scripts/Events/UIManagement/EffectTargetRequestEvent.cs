using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[System.Serializable]
public class EffectTargetRequestEventInfo {
    // If we have more stringent requirements for targeting,
    // we can make this a Dictionary mapping types to predicates
    // that return whether the object of that type is a valid target
    // That feels way too complex to implement right now though
    public List<Type> validTargets;
    public string sourceId;
    public Transform source;

    public EffectTargetRequestEventInfo(List<Type> validTargets, string sourceId, Transform source) {
        this.validTargets = validTargets;
        this.sourceId = sourceId;
        this.source = source;
    }

}

[CreateAssetMenu(
    fileName = "EffectTargetRequestEvent",
    menuName = "Events/Game Event/Effect Target Request Event")]
public class EffectTargetRequestEvent : BaseGameEvent<EffectTargetRequestEventInfo> { }
