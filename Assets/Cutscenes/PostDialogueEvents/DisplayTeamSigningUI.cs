using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTeamSigningUI : MonoBehaviour
{
    [SerializeField]
    private GameObject teamSigningUIGO;
    private TeamSelectionUI teamSelectionUI;

    void Start() {
        teamSelectionUI = teamSigningUIGO.GetComponent<TeamSelectionUI>();
    }

    public void DisplayTeamSigning() {
        teamSelectionUI.toggleDisplay();
        // TODO: Make this play the specific next sequence.
        // This only works for now because we only have two sequences in the scene
        DialogueManager.Instance.StartAnyDialogueSequence();
    }
    
}
