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
        CompanionInstance origin = document.map.GetItem<CompanionInstance>(EffectDocument.ORIGIN, 0);
        if(origin == null) {
            EffectError("No companion instance in origin for dialogue step. Support for other entities is not yet implemented, just gotta write a switch case n shit");
            yield return null;
        }
        yield return GenericEntityDialogueParticipant.Instance.SpeakCompanionLine(line, origin.companion.companionType, lineTime);
        Debug.Log("Dialogue step finished");
    }
}