using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTeamSigningUI : MonoBehaviour
{
    [SerializeField]
    private GameObject teamSigningUIGO;
    private TeamSelectionUI teamSelectionUI;

    void Start() {
        teamSelectionUI = teamSigningUIGO.GetComponentInChildren<TeamSelectionUI>();
        DisplayTeamSigning();
    }

    public void DisplayTeamSigning() {
        Debug.Log("Displaying team signing UI");
        teamSigningUIGO.SetActive(true);
        // TODO: Make this play the specific next sequence.
        // This only works for now because we only have two sequences in the scene
        DialogueManager.Instance.StartAnyDialogueSequence();
    }

}
