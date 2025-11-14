using UnityEngine;
using System.Collections;
using System;

public class TurnPhaseTrigger {
    public TurnPhase phase;
    public IEnumerable triggerResponse;
    public Func<IEnumerator> boundHandler;   // store handler for removal


    public TurnPhaseTrigger(TurnPhase phase, IEnumerable triggerResponse)
    {
        this.phase = phase;
        this.triggerResponse = triggerResponse;
    }
}

[System.Serializable]
public class TurnPhaseTriggerEventInfo {
    public TurnPhaseTrigger turnPhaseTrigger;

    public TurnPhaseTriggerEventInfo (TurnPhaseTrigger newPhaseTrigger){
        this.turnPhaseTrigger = newPhaseTrigger;
    }

    public override string ToString()
    {
        return "TurnPhaseTrigger " + turnPhaseTrigger.phase + " " + turnPhaseTrigger.triggerResponse;
    }
}

[CreateAssetMenu(
    fileName = "NewTurnPhaseTriggerEvent",
    menuName = "Events/Turn Management/Turn Phase Trigger Event")]
public class TurnPhaseTriggerEvent : BaseGameEvent<TurnPhaseTriggerEventInfo> { }
