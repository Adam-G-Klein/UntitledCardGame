using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System;

public class DialogueManager : GenericSingleton<DialogueManager>
{
    private List<DialogueSpeaker> dialogueSpeakers;
    [SerializeField]
    private DialogueLocationSO dialogueLocation;
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
    // So other scripts can check if the dialogue manager is ready
    public bool initialized = false;
    // Wait on the TeamSelectionManager, or the scene equivalent, to set the location
    private bool locationInitialized = false;
    

    public void RegisterDialogueSpeaker(DialogueSpeaker dialogueSpeaker)
    {
        dialogueSpeakers.Add(dialogueSpeaker);
    }

    void Awake() {
        dialogueSpeakers = new List<DialogueSpeaker>();
        initialized = false;

    }

    void Start() {
        // Script execution order in Team select is making this not work: 
        // StartCoroutine(validateSpeakers());
        StartCoroutine(validateDialogueSequences());
        StartCoroutine(initialize());
        // TODO: initiating speakers have bool set
    }

    private IEnumerator initialize() {
        yield return new WaitUntil(() => locationInitialized);
        initialized = true;
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

    public List<DialogueSequenceSO> GetDialogueSequencesWithPresentSpeakers() {
       var presentSpeakers = GetPresentSpeakers();
       foreach(DialogueSequenceSO sequence in dialogueLocation.sequencesAtLocation) {
           Debug.Log("sequence at loc: " + sequence.name);
       }
       return dialogueLocation.sequencesAtLocation
            .Where( (sequence) =>
                 AreAllSpeakersPresent(sequence.requiredSpeakers)).ToList();
    }

    private DialogueSequenceSO GetDialogueSequence() {
        var withPresentSpeakers = GetDialogueSequencesWithPresentSpeakers();
        var unviewed = withPresentSpeakers.Where(sequence => !alreadyViewedSequences.Contains(sequence));
        return unviewed.FirstOrDefault();
    }

    private HashSet<SpeakerTypeSO> GetPresentSpeakers() {
        return dialogueSpeakers.Select(speaker => speaker.speakerType).ToHashSet();
    }


    private bool AreAllSpeakersPresent(List<SpeakerTypeSO> requiredSpeakers) {
        return requiredSpeakers.All(speaker => dialogueSpeakers.Any(s => s.speakerType == speaker));
    }

    public void StartDialogueSequence(DialogueSequenceSO dialogueSequence, Action callback = null) {
        Debug.Log("Starting dialogue sequence: " + dialogueSequence.name);
        currentDialogueSequenceCoroutine = dialogueSequenceCoroutine(dialogueSequence, callback);
        StartCoroutine(currentDialogueSequenceCoroutine);
        alreadyViewedSequences.Add(dialogueSequence);
    }

    
    private IEnumerator dialogueSequenceCoroutine(DialogueSequenceSO dialogueSequence, Action callback = null) {
        dialogueInProgress = true;
        currentDialogueIndex = 0;
        while(currentDialogueIndex < dialogueSequence.dialogueLines.Count) {
            currentLine = dialogueSequence.dialogueLines[currentDialogueIndex];
            // find a speaker in the scene that matches the speaker type of the current line
            currentLineSpeaker = dialogueSpeakers.Where(speaker => speaker.speakerType == currentLine.speaker)
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
        if(callback != null) callback.Invoke();
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
            .Where(speaker => speaker.speakerType == null)) {
            Debug.LogError("Dialogue speaker " + speaker.name + " has no speaker type set.");

        }

        foreach(DialogueSpeaker speaker in dialogueSpeakers.
            Where(speaker => 
                speaker.speakerType.companionType == null
                && !SpeakerTypeSO.NonCompanionSpeakers.Contains(speaker.speakerType.speakerType)))
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

    public void SetDialogueLocation(DialogueLocationSO location) {
        Debug.Log("Setting dialogue location to: " + location.name);
        dialogueLocation = location;
        locationInitialized = true;
    }

    public int GetDialogueSpeakersCount() {
        return dialogueSpeakers.Count;
    }   

}