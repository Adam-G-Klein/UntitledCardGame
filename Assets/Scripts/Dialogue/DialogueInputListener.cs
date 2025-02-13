using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

Separating this out from the DialogueManager because I can imagine a lot of 
logic for determining whether input is actually directed at the dialogue system.
This is where that should live.

We might even want one of these on each of the dialogue boxes if we want input to be there 
specifically. Hence using the instance instead of GetComponent

*/
public class DialogueInputListener : MonoBehaviour
{
    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = DialogueManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            dialogueManager.UserClick();
        }
        if(Input.GetKeyDown(KeyCode.R)) {
            dialogueManager.StartAnyDialogueSequence();
        }
        if(Input.GetKeyDown(KeyCode.J)) {
            dialogueManager.skipCurrentDialogue();
        }
        if(Input.GetKeyDown(KeyCode.K)) {
            Debug.Log("killing player");
            DialogueManager.Instance.gameState.KillPlayer();
        }
    }

}
