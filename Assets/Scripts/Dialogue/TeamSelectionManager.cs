using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using UnityEditor;
using Unity.VisualScripting;

[RequireComponent(typeof(Canvas))]
public class TeamSelectionManager : MonoBehaviour
{

    [SerializeField]
    private GameStateVariableSO gameState;

    [SerializeField]
    private DialogueLocationListSO dialogueLocationList;

    private List<TeamSelectionCompanion> allTeamSelectionCompanions = new List<TeamSelectionCompanion>();

    [SerializeField]
    // I think there's a chance this is just good enough for the demo
    private List<TeamSelectionCompanion> speakersInScene = new List<TeamSelectionCompanion>();
    private Dictionary<SpeakerTypeSO, TeamSelectionCompanion> teamSelectionCompanionBySpeakerType = new Dictionary<SpeakerTypeSO, TeamSelectionCompanion>();

    private List<CompanionTypeSO> presentCompanionTypes = new List<CompanionTypeSO>();

    void Start() 
    {
        int locationIndex = gameState.GetTeamSelectionIndex();
        DialogueManager.Instance.SetDialogueLocation(dialogueLocationList.dialogueLocations[locationIndex]);
        StartCoroutine(initializeCompanions());
        // need scene building logic for instantiating the proper dialogeSpeakers
        // save the length of the speakers list, wait until the manager's list is that long
    }

    private IEnumerator getSpeakersInScene() {
        // Just gonna tag the prefabs and not overthink this
        GameObject[] gos = GameObject.FindGameObjectsWithTag("TeamSelectCompanion");
        List<CompanionTypeSO> presentCompanionTypes = gameState.companions.companionList.Select(companion => companion.companionType).ToList();
        allTeamSelectionCompanions = gos
            .Select(go => go.GetComponent<TeamSelectionCompanion>()).ToList();
        foreach(TeamSelectionCompanion tsc in allTeamSelectionCompanions) {
            if(presentCompanionTypes.Contains(tsc.companionType)) {
                speakersInScene.Add(tsc);
                teamSelectionCompanionBySpeakerType.Add(tsc.companionType.speakerType, tsc);
            } else {
                tsc.Initialize(false, null);
            }
        }
        // yield back to the initialization coroutine
        yield return null;
    }

    private IEnumerator initializeCompanions() {
        yield return StartCoroutine(getSpeakersInScene());
        for(int i = 0; i < gameState.companions.companionList.Count; i++) {
            // instantiate a speaker for each companion
            // (this is a placeholder for now)
            Companion companion = gameState.companions.companionList[i];
            if(companion.companionType.speakerType == null) {
                Debug.LogError("Companion " + companion.companionType.name + " has no speaker type, speaker initialization will fail");
            }
            foreach(TeamSelectionCompanion tsc in speakersInScene) {
                if(tsc.companionType == companion.companionType) {
                    tsc.Initialize(true, companion);
                }
            }
        }
        yield return StartCoroutine(distributeSequencesToSpeakers());
    }

    // wait until the DialogueManager has seen the instantiated dialogueSpeakers
    private IEnumerator distributeSequencesToSpeakers() {
        Debug.Log("distributing sequences to speakers");
        yield return new WaitUntil(() => DialogueManager.Instance.initialized);
        Debug.Log("dialogue manager initialized");
        yield return new WaitUntil(() => 
            DialogueManager.Instance.GetDialogueSpeakersCount() >= gameState.companions.companionList.Count);
        Debug.Log("dialogue manager has at least as many speakers as we have companions");
        List<DialogueSequenceSO> sequencesWithPresentSpeakers = 
            DialogueManager.Instance.GetDialogueSequencesWithPresentSpeakers();
        Debug.Log("dialogue sequences we can display with the speakers present: " + sequencesWithPresentSpeakers.Count);
        foreach(DialogueSequenceSO sequence in sequencesWithPresentSpeakers) {
            // always let the first speaker initiate for now
            // also won't have Aiden initiate any dialogue for now
            SpeakerTypeSO initiator = sequence.dialogueLines.First().speaker;
            if(initiator.speakerType == SpeakerType.Companion) {
                teamSelectionCompanionBySpeakerType[initiator]
                    .SetInitiatableDialogue(sequence);
            }
        }
        

    }


    




}