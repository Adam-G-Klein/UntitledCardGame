using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptView : MonoBehaviour
{
    [SerializeField]
    private Image unclickedImage;
    [SerializeField]
    private Image clickedImage;

    private Button button; 
    private CompanionDialogueParticipant teamSelectionCompanion;
    private bool clicked = false;
    private bool enabledInScene;

    public void InitializeView(bool enabled) {
        enabledInScene = enabled;
        teamSelectionCompanion = GetComponentInParent<CompanionDialogueParticipant>();
        button = GetComponent<Button>();
        if(!enabledInScene) SetVisible(false);
    }

    public void SetVisible(bool visible) {
        // for the case where we don't have a dialogue option to initiate but 
        // we do have dialogue lines to speak
        // in order to know whether we have a sequence to intiiate we need to know whether the sequnece has all its 
        // required speakers.
        // We just need to initiate the prompt after everything else then right?
        if(!enabledInScene) return;
        button.enabled = visible;
        clickedImage.enabled = visible;
        unclickedImage.enabled = visible && !clicked;
    }

    public void OnClick() {
        clicked = true;
        teamSelectionCompanion.StartInitiatableDialogue(() => SetVisible(true));
        SetVisible(false);
    }


}