using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasDisableButtonsAction : TutorialAction
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private List<Button> buttonsToLeaveEnabled;

    private HashSet<Button> leaveEnabledSet = new();

    public CanvasDisableButtonsAction() {
        tutorialActionType = "CanvasDisableButtonsAction";
    }

    public override IEnumerator Invoke() {
        //If this produces a noticable stutter then we can optimize
        Button[] buttonsInCanvas = canvas.GetComponentsInChildren<Button>();

        //convert to hashSet for speed
        foreach (var button in buttonsToLeaveEnabled) {
            leaveEnabledSet.Add(button);
        }

        foreach (var button in buttonsInCanvas) {
           button.interactable = leaveEnabledSet.Contains(button);
        }

        yield return null;
    }
}
