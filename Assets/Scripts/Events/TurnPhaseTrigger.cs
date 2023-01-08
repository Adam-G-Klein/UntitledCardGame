using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPhaseTrigger {
    public TurnPhase phase;
    public bool isFinished = true;
    public TurnPhaseTrigger(TurnPhase phase)
    {
        this.phase = phase;
    }
}