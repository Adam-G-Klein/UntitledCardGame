using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// Tried to get this class to be an extension of 
// a UIStateEventInfo with newState always Effect_Targetting, but it didn't work with our
// event bus
[System.Serializable]
public class EffectTargetRequestEventInfo {
    // If we have more stringent requirements for targeting,
    // we can make this a Dictionary mapping types to predicates
    // that return whether the object of that type is a valid target
    // That feels way too complex to implement right now though
    public string sourceId;
    public Transform source;
    public CardEffectData effect;

    public EffectTargetRequestEventInfo(CardEffectData effect, string sourceId, Transform source){
        this.effect = effect;
        this.sourceId = sourceId;
        this.source = source;
    }

}

[CreateAssetMenu(
    fileName = "EffectTargetRequestEvent",
    menuName = "Events/Game Event/Effect Target Request Event")]
public class EffectTargetRequestEvent : BaseGameEvent<EffectTargetRequestEventInfo> { }
