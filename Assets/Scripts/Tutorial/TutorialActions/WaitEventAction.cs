using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//lets see how this works
public class TutorialWaitListener : IGameEventListener<object>
{
    private System.Action m_action;
    public TutorialWaitListener(System.Action action) {
        m_action = action;
    }

    public void OnEventRaised(object item) {
        //may unpack later
        m_action?.Invoke();
    }
}


public class WaitEventAction : TutorialAction
{
    public TutorialEvent tutorialEvent;

    private bool isReady = false;

    public WaitEventAction() {
        tutorialActionType = "Event Wait Action";
    }

    public override IEnumerator Invoke() {

        var waitListener = new TutorialWaitListener(completedEventWait);

        tutorialEvent.RegisterListener(waitListener);

        yield return new WaitUntil(() => isReady);

        tutorialEvent.UnregisterListener(waitListener);
    }

    private void completedEventWait() {
        isReady = true;
    }
}
