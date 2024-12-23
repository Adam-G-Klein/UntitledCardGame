using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForNextButtonClickAction : TutorialAction
{
    private bool buttonClicked = false;

    public WaitForNextButtonClickAction() {
        tutorialActionType = "Wait For Next Button Click Action";
    }

    public override IEnumerator Invoke() {
        yield return new WaitUntil(() => buttonClicked);
    }
    
    public void ButtonClicked() {
        buttonClicked = true;
    }
}
