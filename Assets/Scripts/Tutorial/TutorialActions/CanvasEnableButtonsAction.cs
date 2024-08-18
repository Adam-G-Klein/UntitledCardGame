using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasEnableButtonsAction : TutorialAction
{
    [SerializeField]
    private Canvas canvas;

    public CanvasEnableButtonsAction() {
        tutorialActionType = "Canvas Enable Buttons Action";
    }

    public override IEnumerator Invoke() {
        //If this produces a noticable stutter then we can optimize
        Button[] buttonsInCanvas = canvas.GetComponentsInChildren<Button>();

        foreach (var button in buttonsInCanvas) {
            button.interactable = true;
        }

        yield return null;
    }
}