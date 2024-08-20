using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericEntityDialogueParticipant : GenericSingleton<GenericEntityDialogueParticipant> 
{
    // initialize and instantiate here, have this initialize the dialogueSpeaker
    private DialogueSpeaker dialogueSpeaker;

    void Start() {
        dialogueSpeaker = GetComponentInChildren<DialogueSpeaker>();
        dialogueSpeaker.InitializeSpeaker(false);
    }

    public IEnumerator SpeakCompanionLine(string line, CompanionTypeSO speaker, float timeAfterLine = 1.0f) {
        yield return StartCoroutine(dialogueSpeaker.SpeakLine(line, speaker, timeAfterLine));
        yield return new WaitForSeconds(timeAfterLine);
    }

}