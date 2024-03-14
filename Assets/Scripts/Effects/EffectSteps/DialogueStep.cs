using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class DialogueStep : EffectStep {
    public DialogueLocationSO dialogueLocationSO;
    public DialogueStep() {
        effectStepName = "DialogueStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        DialogueManager.Instance.StartDialogueSequenceFromLocation(dialogueLocationSO);
        yield return null;
    }
}