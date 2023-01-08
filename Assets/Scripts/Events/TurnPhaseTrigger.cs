using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPhaseTrigger {
    public TurnPhase phase;
    public IEnumerable triggerResponse;

    public TurnPhaseTrigger(TurnPhase phase, IEnumerable triggerResponse)
    {
        this.phase = phase;
        this.triggerResponse = triggerResponse;
    }
}