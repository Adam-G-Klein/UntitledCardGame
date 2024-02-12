using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionCompanion : MonoBehaviour 
{
    // initialize and instantiate here, have this initialize the dialogueSpeaker
    [SerializeField]
    private Companion companion;
    private DialogueSpeaker dialogueSpeaker;
    [SerializeField]
    private Image image;

    public void Initialize(Companion companion) {
        this.companion = companion;
        dialogueSpeaker = GetComponentInChildren<DialogueSpeaker>();
        dialogueSpeaker.InitializeSpeaker(companion.companionType.speakerType);
        image.sprite = companion.companionType.sprite;
    }

}