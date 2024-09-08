using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitAction : TutorialAction
{
    public float timeToWait;

    public WaitAction() {
        tutorialActionType = "Wait Action";
    }

    public override IEnumerator Invoke() {
        Debug.Log("==== Debug tutorial action: " + tutorialActionType + " ====");

        yield return new WaitForSeconds(timeToWait);
    }
}
