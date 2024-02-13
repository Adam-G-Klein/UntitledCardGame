using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionCompanion : MonoBehaviour 
{
    // initialize and instantiate here, have this initialize the dialogueSpeaker
    [Tooltip("This companion's type, used to set the image in the scene. Searched for by the TeamSelectionManager")]
    public CompanionTypeSO companionType;
    [SerializeField]
    private Companion companion;
    private DialogueSpeaker dialogueSpeaker;
    [SerializeField]
    [Tooltip("This image is set from this component's companionType, using its sprite field")]
    private Image image;
    private InteractionPromptView interactionPromptView;
    private DialogueSequenceSO initiatableDialogue;


    public void Initialize(bool enabled, Companion companion = null) {
        interactionPromptView = GetComponentInChildren<InteractionPromptView>();
        dialogueSpeaker = GetComponentInChildren<DialogueSpeaker>();
        interactionPromptView.InitializeView(enabled);
        if(!enabled) return;
        this.companion = companion;
        image.sprite = companion.companionType.sprite;
        image.enabled = enabled;
        dialogueSpeaker.InitializeSpeaker(enabled, 
            companion.companionType.speakerType, 
            interactionPromptView);
    }


    public void SetInitiatableDialogue(DialogueSequenceSO dialogueSequence) {
        Debug.Log("TSC " + gameObject.name + " Setting initiatable dialogue: " + dialogueSequence.name);
        interactionPromptView.SetVisible(true);
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