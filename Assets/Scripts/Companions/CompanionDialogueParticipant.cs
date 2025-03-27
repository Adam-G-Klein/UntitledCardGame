using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanionDialogueParticipant : MonoBehaviour
{
    // initialize and instantiate here, have this initialize the dialogueSpeaker
    [Tooltip("This companion's type, used to set the image in the scene. Searched for by the TeamSelectionManager")]
    public CompanionTypeSO companionType;
    [SerializeField]
    private Companion companion;
    private DialogueSpeaker dialogueSpeaker;
    [SerializeField]
    [Tooltip("This image is set from this component's companionType, using its sprite field")]
    private Image spriteInScene;
    private InteractionPromptView interactionPromptView;
    private DialogueSequenceSO initiatableDialogue;


    public void InitializeCompanion(bool enabled, Location currentLocation, Companion companion = null) {
        // Get the components so that we don't null ptr in any erroneous future calls
        dialogueSpeaker = GetComponentInChildren<DialogueSpeaker>();
        if(!enabled) {
            dialogueSpeaker.InitializeSpeaker(enabled);
            return;
        }
        this.companion = companion;
        if(currentLocation == Location.TEAM_SELECT) {
            spriteInScene.sprite = companion.companionType.sprite;
            spriteInScene.enabled = enabled;
        }
        dialogueSpeaker.InitializeSpeaker(enabled,
            companion.companionType.speakerType,
            interactionPromptView);
    }

    // have to do this as a second step, the dialogue manager doesn't know
    // if we'll have a dialogue to initiate before it gets all of the
    // speakers initialized and registered
    public void InitializePromptView(bool enabled, DialogueSequenceSO dialogueSequence) {
        Debug.Log("Initializing prompt view. Enabled: " + enabled + " dialogueSequence: " + dialogueSequence);
        if(!enabled) return;
        interactionPromptView = GetComponentInChildren<InteractionPromptView>();
        if(interactionPromptView) {
            interactionPromptView.InitializeView(enabled);
            interactionPromptView.SetVisible(enabled);
        }
        initiatableDialogue = dialogueSequence;
    }

    public void StartInitiatableDialogue(Action callback = null) {
        if(initiatableDialogue != null) {
            // prompt will be re-displayed again by the callback the view passes us
            // it's messy but hey iteration 1 right
            DialogueManager.Instance.StartDialogueSequence(initiatableDialogue, callback);
        }
    }

}