using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

// only set up to display dialogue as a part of companion abilities right now
public class DialogueStep : EffectStep {
    public string line;
    public float lineTime = 1.0f;
    public DialogueStep() {
        effectStepName = "DialogueStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        CompanionInstance instanceOrigin = document.map.TryGetItem<CompanionInstance>(EffectDocument.ORIGIN, 0);
        Companion companionOrigin = document.map.TryGetItem<Companion>(EffectDocument.ORIGIN, 0);
        if(instanceOrigin != null) {
            yield return GenericEntityDialogueParticipant.Instance.SpeakCompanionLine(line, instanceOrigin.companion.companionType, lineTime);
        } else if (companionOrigin != null) {
            yield return GenericEntityDialogueParticipant.Instance.SpeakCompanionLine(line, companionOrigin.companionType, lineTime);
        } else {
            Debug.LogError("DialogueStep: No companion origin found");
        }
        Debug.Log("Dialogue step finished");
    }
}