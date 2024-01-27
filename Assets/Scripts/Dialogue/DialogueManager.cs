using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

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
    // TODO: make this configurable on a per-sequence basis
    // This was the fastest way I could think of to make the team signing sequence work
    // Putting this here for reference: https://forum.unity.com/threads/playing-a-coroutine-in-timeline.982128/
    // Could be a good starting place for our second iteration dialogue system if we end up needing it
    [SerializeField]
    private List<UnityEvent> postSequenceEvents = new List<UnityEvent>();
    [SerializeField]
    private int nextPostSequenceEventIndex = 0;
    [SerializeField]
    private DialogueSequenceSO tutorialSequence;
    

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
        Debug.Log("Starting any dialogue sequence");
        if(dialogueInProgress) return;
        
        DialogueSequenceSO toStart = GetDialogueSequence();
        // laughing at myself right now. first iteration first iteration its just a first iteration
        // TODO - do this sensibly
        if(toStart == tutorialSequence) {
            if(EnemyEncounterManager.Instance.gameState.playerData.GetValue().seenTutorial) {
                Debug.Log("Player has seen tutorial, not starting any dialogue sequence");
                return;
            } else {
                Debug.Log("Player has not seen tutorial, starting tutorial");
                EnemyEncounterManager.Instance.gameState.playerData.GetValue().seenTutorial = true;
            }
        }
        
        if(toStart != null)
            StartDialogueSequence(toStart);
    }

    private DialogueSequenceSO GetDialogueSequence() {
        var withPresentSpeakers = GetDialogueSequencesWithPresentSpeakers();
        var unviewed = withPresentSpeakers.Where(sequence => !alreadyViewedSequences.Contains(sequence));
        return unviewed.FirstOrDefault();
    }

    private HashSet<SpeakerTypeSO> GetPresentSpeakers() {
        return dialogueSpeakers.Select(speaker => speaker.speaker).ToHashSet();
    }

    private List<DialogueSequenceSO> GetDialogueSequencesWithPresentSpeakers() {
       var presentSpeakers = GetPresentSpeakers();
       return dialogueLocation.sequencesAtLocation
            .Where( (sequence) =>
                 AreAllSpeakersPresent(sequence.requiredSpeakers)).ToList();
    }

    private bool AreAllSpeakersPresent(List<SpeakerTypeSO> requiredSpeakers) {
        return requiredSpeakers.All(speaker => dialogueSpeakers.Any(s => s.speaker == speaker));
    }

    private void StartDialogueSequence(DialogueSequenceSO dialogueSequence) {
        Debug.Log("Starting dialogue sequence: " + dialogueSequence.name);
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
        if(nextPostSequenceEventIndex < postSequenceEvents.Count) {
            postSequenceEvents[nextPostSequenceEventIndex].Invoke();
            nextPostSequenceEventIndex += 1;
        }
    }
    public void UserClick()
    {
        if(currentLineSpeaker != null) {
            // Current line speaker will either fast forward or finish waiting its coroutine,
            // yielding for the next line to be spoken in dialogueSequenceCoroutine above
            currentLineSpeaker.UserButtonClick();
        }
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
        int usableSequences = 0;
        foreach(DialogueSequenceSO s in dialogueLocation.sequencesAtLocation) {
            if(AreAllSpeakersPresent(s.requiredSpeakers)) {
                usableSequences += 1;
            }
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
        if(usableSequences == 0) {
            Debug.LogError("No dialogue sequences found for present speakers at location: " + dialogueLocation.name);
        }
    }
}