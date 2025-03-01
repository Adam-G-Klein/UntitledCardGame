using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(
    fileName = "NewDialogueSequence", 
    menuName = "Dialogue/Dialogue Sequence")]
[System.Serializable]
public class DialogueSequenceSO: ScriptableObject
{
    public List<SpeakerTypeSO> requiredSpeakers; // Can make this into a list later if we want
    [SerializeReference]
    public List<DialogueLine> dialogueLines;
}