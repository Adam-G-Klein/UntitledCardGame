using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DebugAction : TutorialAction
{
    public string debugString;

    public DebugAction() {
        tutorialActionType = "DebugAction";
    }

    public override IEnumerator Invoke(){
        Debug.Log("==== Debug tutorial action: " + debugString + " ====");
        yield return null;
    }

}
