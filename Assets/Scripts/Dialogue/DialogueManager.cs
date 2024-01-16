using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DialogueManager : GenericSingleton<DialogueManager>
{
    private List<DialogueSpeaker> dialogueSpeakers;
    public DialogueLocationSO dialogueLocation;
    public bool dialogueInProgress = false;
    [SerializeReference]
    private DialogueLine currentLine;
    private int currentDialogueIndex = 0;
    private DialogueSpeaker currentLineSpeaker;
    public IEnumerator currentDialogueSequenceCoroutine;
    private List<DialogueSequenceSO> alreadyViewedSequences = new List<DialogueSequenceSO>();

    public void RegisterDialogueSpeaker(DialogueSpeaker dialogueSpeaker)
    {
        dialogueSpeakers.Add(dialogueSpeaker);
    }

    void Awake() {
        dialogueSpeakers = new List<DialogueSpeaker>();
    }

    void Start() {
        StartCoroutine(validateSpeakers());
        StartCoroutine(validateDialogueSequences());
    }

    // For use cases like the shop, post-combat screen, pre-combat screen.
    // Pulls any dialogue sequence from the location 
    // that hasn't been viewed yet and has the required speaker present
    public void StartAnyDialogueSequence() {
        if(dialogueInProgress) return;
        DialogueSequenceSO toStart = GetDialogueSequence();
        if(toStart != null)
            StartDialogueSequence(toStart);
    }

    private DialogueSequenceSO GetDialogueSequence() {
        var presentSpeakers = dialogueSpeakers.Select(speaker => speaker.speaker).ToHashSet();
        return dialogueLocation.sequencesAtLocation
            .Where(sequence => !alreadyViewedSequences.Contains(sequence)
                // check that all of the required speakers are present
                && sequence.requiredSpeakers.All(speaker => presentSpeakers.Contains(speaker)))
            .FirstOrDefault();
    }

    private void StartDialogueSequence(DialogueSequenceSO dialogueSequence) {
        currentDialogueSequenceCoroutine = dialogueSequenceCoroutine(dialogueSequence);
        StartCoroutine(currentDialogueSequenceCoroutine);
        alreadyViewedSequences.Add(dialogueSequence);
    }

    
    private IEnumerator dialogueSequenceCoroutine(DialogueSequenceSO dialogueSequence) {
        dialogueInProgress = true;
        currentDialogueIndex = 0;
        while(currentDialogueIndex < dialogueSequence.dialogueLines.Count) {
            currentLine = dialogueSequence.dialogueLines[currentDialogueIndex];
            // find a speaker in the scene that matches the speaker type of the current line
            currentLineSpeaker = dialogueSpeakers.Where(speaker => speaker.speaker == currentLine.speaker)
                .FirstOrDefault();
            if(currentLineSpeaker != null) {
                // will wait until the line is done displaying and the player has provided input
                // right now, the manager passes input to the speaker in NextDialogue()
                yield return StartCoroutine(currentLineSpeaker.SpeakLine(currentLine));
            }
            currentDialogueIndex += 1;
        }
        dialogueInProgress = false;
    }
    public void UserClick()
    {
        if(currentLineSpeaker != null)
            currentLineSpeaker.UserButtonClick();
    }

    private IEnumerator validateSpeakers() {
        yield return new WaitForEndOfFrame();
        if(dialogueSpeakers.Count == 0) {
            Debug.LogError("No dialogue speakers registered. " + 
                " Do we need a dialogue manager in this scene? " + 
                " Or do we still need to place the DialogueSpeaker prefabs in it?");
        }
        foreach(DialogueSpeaker speaker in dialogueSpeakers
            .Where(speaker => speaker.speaker == null)) {
            Debug.LogError("Dialogue speaker " + speaker.name + " has no speaker type set.");

        }

        foreach(DialogueSpeaker speaker in dialogueSpeakers.
            Where(speaker => 
                speaker.speaker.companionType == null
                && !SpeakerTypeSO.NonCompanionSpeakers.Contains(speaker.speaker.speakerType)))
                {
                    Debug.LogError("Dialogue speaker " + speaker.name + " (with parent) " + speaker.transform.parent.name + " has no companion type set.");
                }

    }

    private IEnumerator validateDialogueSequences() {
        yield return new WaitForEndOfFrame();
        if(dialogueLocation.sequencesAtLocation.Count == 0) {
            yield break;
        }
        foreach(DialogueSequenceSO s in dialogueLocation.sequencesAtLocation) {
            s.requiredSpeakers.ForEach(speaker => {
                if(speaker == null) {
                    Debug.LogError("Dialogue sequence " + s.name + " has a null speaker type in its required speakers.");
                }
            });
            s.dialogueLines.ForEach(line => {
                if(line == null) {
                    Debug.LogError("Dialogue sequence " + s.name + " has a null dialogue line.");
                }
            });
        }
    }
}