using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialActionName {
    DebugAction,
    ButtonAction,
    EventAction,
    WaitAction,
    WaitEventAction,
    DialogueAction,
    CanvasDisableButtonsAction,
    CanvasEnableButtonsAction
}

/* The base class for the individual actions a tutorialStep
can incite*/

[System.Serializable]
public abstract class TutorialAction
{
    [HideInInspector]
    public string tutorialActionType;

    public virtual IEnumerator Invoke(){
        yield return null;
    }

    protected void StepError(string errorString) {
        Debug.LogError(tutorialActionType + " TutorialAction: " + errorString);
    }

}
