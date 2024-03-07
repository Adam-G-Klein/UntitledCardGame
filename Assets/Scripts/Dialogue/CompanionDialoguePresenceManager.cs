using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using UnityEditor;
using Unity.VisualScripting;
using System.Threading;

public class CompanionDialoguePresenceManager : MonoBehaviour
{

    [SerializeField]
    private GameStateVariableSO gameState;

    private List<CompanionDialogueParticipant> allCompanionsInScene = new List<CompanionDialogueParticipant>();

    [SerializeField]
    // I think there's a chance this is just good enough for the demo
    private List<CompanionDialogueParticipant> speakersInScene = new List<CompanionDialogueParticipant>();
    private Dictionary<SpeakerTypeSO, CompanionDialogueParticipant> participantCompanionBySpeakerType = new Dictionary<SpeakerTypeSO, CompanionDialogueParticipant>();

    private List<CompanionTypeSO> presentCompanionTypes = new List<CompanionTypeSO>();

    [Header("The amount of time we'll wait for initialization to succeed")]
    public float initializationTimeout = 2;
    private float initStartTime = 0f;

    private Dictionary<SpeakerTypeSO, DialogueSequenceSO> initiatableSequencesBySpeaker = new Dictionary<SpeakerTypeSO, DialogueSequenceSO>();

    void Start() 
    {
        initStartTime = Time.time;
        StartCoroutine(Initialize());
    }

    private IEnumerator initializeDialogueManagerLocation() {
        int locationIndex = gameState.GetLoopIndex();
        Debug.Log("initializing dialogue manager location, seen tutorial? " + gameState.playerData.GetValue().seenTutorial);
        DialogueLocationSO loc = gameState.dialogueLocations.GetDialogueLocation(
            gameState.currentLocation, 
            gameState.GetLoopIndex(), 
            gameState.playerData.GetValue().seenTutorial);
        DialogueManager.Instance.SetDialogueLocation(loc);
        Debug.Log("dialogue manager location initialized");
        
        yield return null;

    }
    
    private IEnumerator getSpeakersInScene() {
        // Just gonna tag the prefabs and not overthink this
        GameObject[] gos = GameObject.FindGameObjectsWithTag("CompanionDialogueParticipant");
        List<CompanionTypeSO> presentCompanionTypes = gameState.companions.allCompanions.Select(companion => companion.companionType).ToList();
        allCompanionsInScene = gos
            .Select(go => go.GetComponent<CompanionDialogueParticipant>()).ToList();
        foreach(CompanionDialogueParticipant cdp in allCompanionsInScene) {
            if(presentCompanionTypes.Contains(cdp.companionType)) {
                speakersInScene.Add(cdp);
                participantCompanionBySpeakerType.Add(cdp.companionType.speakerType, cdp);
            } else {
                cdp.InitializeCompanion(false, gameState.currentLocation, null);
            }
        }
        // yield back to the initialization coroutine
        yield return null;
    }

    private IEnumerator initializeSpeakers() {
        List<Companion> presentCompanions = gameState.companions.allCompanions;
        foreach(Companion companion in presentCompanions) {
            if(companion.companionType.speakerType == null) {
                Debug.LogError("Companion " + companion.companionType.name + " has no speaker type, speaker initialization will fail");
            }
            foreach(CompanionDialogueParticipant cdp in speakersInScene) {
                if(cdp.companionType == companion.companionType) {
                    cdp.InitializeCompanion(true, gameState.currentLocation, companion);
                }
            }
        }
        yield return null;
    }

    private IEnumerator Initialize() {
        yield return StartCoroutine(initializeDialogueManagerLocation());
        yield return StartCoroutine(getSpeakersInScene());
        yield return StartCoroutine(initializeSpeakers());
        // wait for all the companions to report in
        yield return new WaitUntil(() => 
            (DialogueManager.Instance.GetDialogueSpeakersCount() >= gameState.companions.activeCompanions.Count)
            || Time.time - initStartTime > initializationTimeout );
        int cnt = 0;
        foreach(DialogueSpeaker speaker in DialogueManager.Instance.GetDialogueSpeakers()) {
            Debug.Log("registered dialogue speaker: " + speaker.speakerType);
            cnt += 1;
        }
        Debug.Log("registered " + cnt + " dialogue speakers, expected " + gameState.companions.activeCompanions.Count);
        if(Time.time - initStartTime > initializationTimeout) {
            Debug.LogError("Dialogue initialization failed timeout");
        }
        DialogueManager.Instance.SetSpeakersInitialized();
        if(gameState.currentLocation == Location.TEAM_SELECT)
            yield return StartCoroutine(distributeSequencesToSpeakers());
        else yield return null;
    }

    // wait until the DialogueManager has seen the instantiated dialogueSpeakers
    private IEnumerator distributeSequencesToSpeakers() {
        
        // TODO: add check for aiden / the concierge. Should just be a + 2 in the check
        Debug.Log("dialogue manager has at least as many speakers as we have companions");
        List<DialogueSequenceSO> sequencesWithPresentSpeakers = 
            DialogueManager.Instance.GetDialogueSequencesWithPresentSpeakers();
        Debug.Log("dialogue sequences we can display with the speakers present: " + sequencesWithPresentSpeakers.Count);
        foreach(CompanionDialogueParticipant cdp in speakersInScene) {
            // TODO, random sequence selection
            DialogueSequenceSO sequence = sequencesWithPresentSpeakers
                .FirstOrDefault(s => s.dialogueLines.First().speaker == cdp.companionType.speakerType);
            if(sequence != null) {
                cdp.InitializePromptView(true, sequence);
            } else {
                cdp.InitializePromptView(false, null);
            }
        }
        yield return null;
    }
    
}