using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForTurnPhaseAction : TutorialAction
{
    public TurnPhase turnPhase;

    private bool phaseReached = false;

    public WaitForTurnPhaseAction() {
        tutorialActionType = "Wait For Turn Phase Action";
    }

    public override IEnumerator Invoke() {
        yield return new WaitUntil(() => phaseReached);
    }
    
    public void OnTurnPhaseChange(TurnPhase phase) {
        if (phase == turnPhase) {
            phaseReached = true;
        }
    }
}
