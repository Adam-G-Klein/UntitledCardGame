using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventAction : TutorialAction
{
    public UnityEvent Event;

    public EventAction() {
        tutorialActionType = "Event Action";
    }

    public override IEnumerator Invoke() {
        Debug.Log("==== Debug tutorial action: " + tutorialActionType + " ====");

        Event?.Invoke();

        yield return null;
    }
}
