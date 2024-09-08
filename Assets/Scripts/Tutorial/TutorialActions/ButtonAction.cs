using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ButtonAction : TutorialAction
{

    public UnityEngine.UI.Button Button;


    private bool hasPressed = false;
    public ButtonAction()
    {
        tutorialActionType = "Button Action";
    }

    public override IEnumerator Invoke() {
        Debug.Log("==== Debug tutorial action: " + tutorialActionType + " ====");

        //Register with the button
        //could cause an issue, potentially look into better solution, ensure the button is hidden until it is time to use it
        Button.onClick.AddListener(HandleClick);

        //For now we must poll
        yield return new WaitUntil(() => hasPressed);
    }

    private void HandleClick() {
        hasPressed = true;
    }
}
