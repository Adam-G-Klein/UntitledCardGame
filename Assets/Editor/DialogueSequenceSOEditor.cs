using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueSequenceSO))]
public class DialogueSequenceSOEditor : Editor {

    public override void OnInspectorGUI() {
        DialogueSequenceSO dialogueSequence = (DialogueSequenceSO) target;
        DrawDefaultInspector();

        if (GUILayout.Button("Add Line")) {
            DialogueLine newLine = new DialogueLine();
            dialogueSequence.dialogueLines.Add(newLine);

        }
    }
}