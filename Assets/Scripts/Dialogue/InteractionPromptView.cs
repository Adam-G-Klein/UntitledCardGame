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
    private TeamSelectionCompanion teamSelectionCompanion;
    private bool clicked = false;

    public void InitializeView(bool enabled) {
        teamSelectionCompanion = GetComponentInParent<TeamSelectionCompanion>();
        button = GetComponent<Button>();
        if(!enabled) SetVisible(false);
    }

    public void SetVisible(bool visible) {
        print("prompt visible called " + visible);
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