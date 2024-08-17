using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAction : TutorialAction
{
    [SerializeField]
    public DialogueSequenceSO dialogueSequenceSO;

    private bool doneSpeaking = false;

    public DialogueAction() {
        tutorialActionType = "Dialouge Action";
    }

    public override IEnumerator Invoke() {
        Debug.Log("==== Debug tutorial action: " + tutorialActionType + " ====");

        DialogueManager.Instance.StartDialogueSequence(dialogueSequenceSO, Done);

        //For now we must poll
        yield return new WaitUntil(() => doneSpeaking);
    }

    private void Done() {
        doneSpeaking = true;
    }
}
