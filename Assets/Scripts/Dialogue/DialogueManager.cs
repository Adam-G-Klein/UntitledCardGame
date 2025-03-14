using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System;

public class DialogueManager : GenericSingleton<DialogueManager>
{
    private List<DialogueSpeaker> dialogueSpeakers;
    public GameStateVariableSO gameState;
    [SerializeField]
    private DialogueLocationSO dialogueLocation;
    public bool dialogueInProgress = false;
    [SerializeReference]
    private DialogueLine currentLine;
    private int currentDialogueIndex = 0;
    private DialogueSpeaker currentLineSpeaker;
    public IEnumerator currentDialogueSequenceCoroutine;
    private Action currentDialogueSequenceCallback;
    // TODO: make this configurable on a per-sequence basis
    // This was the fastest way I could think of to make the team signing sequence work
    // Putting this here for reference: https://forum.unity.com/threads/playing-a-coroutine-in-timeline.982128/
    // Could be a good starting place for our second iteration dialogue system if we end up needing it
    [SerializeField]
    private List<UnityEvent> postSequenceEvents = new List<UnityEvent>();
    [SerializeField]
    private int nextPostSequenceEventIndex = 0;
    // So other scripts can check if the dialogue manager is ready
    public bool initialized { 
        get {return locationInitialized && speakersInitialized;}} 
    // Wait on the TeamSelectionManager, or the scene equivalent, to set the location
    private bool locationInitialized = false;
    private bool speakersInitialized = false;
    private GenericEntityDialogueParticipant genericEntityDialogueParticipant;
    

    public void RegisterDialogueSpeaker(DialogueSpeaker dialogueSpeaker)
    {
        dialogueSpeakers.Add(dialogueSpeaker);
    }

    void Awake() {
        dialogueSpeakers = new List<DialogueSpeaker>();
    }

    void Start() {
        // Script execution order in Team select is making this not work: 
        // StartCoroutine(validateSpeakers());
        StartCoroutine(initialize());
        // TODO: initiating speakers have bool set
    }

    private IEnumerator initialize() {
        yield return new WaitUntil(() => locationInitialized);
        yield return new WaitUntil(() => speakersInitialized);
        StartCoroutine(validateDialogueSequences());
    }

    // For use cases like the shop, post-combat screen, pre-combat screen.
    // Pulls any dialogue sequence from the location 
    // that hasn't been viewed yet and has the required speaker present
    public void StartAnyDialogueSequence(float delay = 0f) {
        Debug.Log("Starting any dialogue sequence");
        if(dialogueInProgress) {
            // TODO: make it possible to enqueue dialogue
            Debug.LogWarning("Dialogue already in progress, not starting any dialogue sequence");
            return;
        } 
        // hey man I'm just tryna make a living out here yknow what I mean
        StartCoroutine(runOnceInitialized( () => {
            DialogueSequenceSO toStart = GetDialogueSequence();
            if(toStart != null)
                StartDialogueSequence(toStart);
        }, delay));
    }

    public void StartDialogueSequenceFromLocation(DialogueLocationSO location, float delay = 0f) {
        Debug.Log("Starting dialogue sequence from location: " + location.name);
        StartCoroutine(runOnceInitialized( () => {
            // choose a random element from the location.sequences list
            DialogueSequenceSO toStart = location.sequencesAtLocation[UnityEngine.Random.Range(0, location.sequencesAtLocation.Count)];
            if(toStart != null)
                StartDialogueSequence(toStart);
        }, delay));
    }

    private IEnumerator runOnceInitialized(Action callback, float delay = 0f) {
        yield return new WaitUntil(() => initialized);
        yield return new WaitForSeconds(delay);
        callback.Invoke();
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
        List<DialogueSequenceSO> unviewed = new List<DialogueSequenceSO>();

            // bro don't make fun of me its the day before GDC
        foreach(DialogueSequenceSO sequence in withPresentSpeakers) {
            bool seqViewed = false;
            foreach(DialogueSequenceSO viewed in gameState.viewedSequences) {
                if(sequence.name == viewed.name) {
                    seqViewed = true;
                    Debug.Log("sequence " + sequence.name + " has already been viewed");
                } 
            }
            if(!seqViewed) {
                unviewed.Add(sequence);
            }
        }
        foreach(DialogueSequenceSO unview in unviewed) {
            Debug.Log("unviewed sequence with present speakers: " + unview.name);
        }
        if(unviewed.Count() == 0) return null;
        return unviewed.FirstOrDefault();
    }

    private HashSet<SpeakerTypeSO> GetPresentSpeakers() {
        return dialogueSpeakers.Select(speaker => speaker.speakerType).ToHashSet();
    }


    private bool AreAllSpeakersPresent(List<SpeakerTypeSO> requiredSpeakers) {
        return requiredSpeakers.All(speaker => dialogueSpeakers.Any(s => s.speakerType == speaker));
    }

    public void StartDialogueSequence(DialogueSequenceSO dialogueSequence, Action callback = null, bool fromTutorial = false) {
        if (TutorialManager.Instance.IsTutorialPlaying && !fromTutorial) return;
        Debug.Log("Starting dialogue sequence: " + dialogueSequence.name);
        currentDialogueSequenceCoroutine = dialogueSequenceCoroutine(dialogueSequence, callback);
        currentDialogueSequenceCallback = callback;
        StartCoroutine(currentDialogueSequenceCoroutine);
        if(gameState && !gameState.debugSingleEncounterMode) {
            gameState.viewedSequences.Add(dialogueSequence);
            Debug.Log("Added " + dialogueSequence.name + " to already viewed sequences");
        }
    }

    
    private IEnumerator dialogueSequenceCoroutine(DialogueSequenceSO dialogueSequence, Action callback = null) {
        dialogueInProgress = true;
        currentDialogueIndex = 0;
        Debug.Log("Starting dialogue sequence coroutine: " + dialogueSequence.name);
        while(currentDialogueIndex < dialogueSequence.dialogueLines.Count) {
            currentLine = dialogueSequence.dialogueLines[currentDialogueIndex];
            // find a speaker in the scene that matches the speaker type of the current line
            currentLineSpeaker = dialogueSpeakers.Where(speaker => speaker.speakerType == currentLine.speaker)
                .FirstOrDefault();
            if(currentLineSpeaker != null) {
                // will wait until the line is done displaying and the player has provided input
                // right now, the manager passes input to the speaker in NextDialogue()
                Debug.Log("Starting line speaker coroutine for speaker " + currentLineSpeaker.speakerType.name);
                yield return StartCoroutine(currentLineSpeaker.SpeakLine(currentLine));
            }
            currentDialogueIndex += 1;
        }
        dialogueFinished();
        
    }

    public void skipCurrentDialogue() {
        // sue me :) - okay litigation incoming
        if(!dialogueInProgress) return;
        currentLineSpeaker.SkipLine();
        StopCoroutine(currentDialogueSequenceCoroutine);
        dialogueFinished();
    }

    private void dialogueFinished() {
        dialogueInProgress = false;
        if(nextPostSequenceEventIndex < postSequenceEvents.Count) {
            Debug.Log("invoking post sequence event " + nextPostSequenceEventIndex + " of " + postSequenceEvents.Count);
            postSequenceEvents[nextPostSequenceEventIndex].Invoke();
            nextPostSequenceEventIndex += 1;
        }
        if(currentDialogueSequenceCallback != null) currentDialogueSequenceCallback.Invoke();
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
            if(s.requiredSpeakers.Count > 0 && !s.requiredSpeakers.Contains(s.dialogueLines.First().speaker)) {
                Debug.LogWarning("Dialogue sequence " + s.name + " has a first line speaker that is not in its required speakers. This will cause issues if it's displayed in the team selection screen, where that speaker must initiate the conversation");
            }
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

    public void SetDialogueLocation(GameStateVariableSO gameState) {
        dialogueLocation = gameState.dialogueLocations.GetDialogueLocation(gameState);

    }
    public void SetSpeakersInitialized() {
        speakersInitialized = true;
    }

    public int GetDialogueSpeakersCount() {
        return dialogueSpeakers.Count;
    }   

    public List<DialogueSpeaker> GetDialogueSpeakers() {
        return dialogueSpeakers;
    }

}