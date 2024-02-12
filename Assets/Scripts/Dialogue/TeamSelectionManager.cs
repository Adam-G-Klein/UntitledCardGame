using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using UnityEditor.iOS.Extensions.Common;
using UnityEditor;
using Unity.VisualScripting;

[RequireComponent(typeof(Canvas))]
public class TeamSelectionManager : MonoBehaviour
{

    [SerializeField]
    private GameStateVariableSO gameState;

    [SerializeField]
    private DialogueLocationListSO dialogueLocationList;

    [SerializeField]
    private GameObject teamSelectSpeakerPrefab;

    [SerializeField]
    // I think there's a chance this is just good enough for the demo
    private List<Transform> speakerLocations = new List<Transform>();
    private List<Vector2> speakerPositions = new List<Vector2>();


    void Start() 
    {
        int locationIndex = gameState.GetTeamSelectionIndex();
        DialogueManager.Instance.SetDialogueLocation(dialogueLocationList.dialogueLocations[locationIndex]);
        StartCoroutine(instantiateCompanions());
        DialogueManager.Instance.StartAnyDialogueSequence();
        // need scene building logic for instantiating the proper dialogeSpeakers
        // save the length of the speakers list, wait until the manager's list is that long
    }

    private IEnumerator instantiateCompanions() {
        initializeSpeakerPositions();
        for(int i = 0; i < gameState.companions.companionList.Count; i++) {
            // instantiate a speaker for each companion
            // (this is a placeholder for now)
            Companion companion = gameState.companions.companionList[i];
            if(companion.companionType.speakerType == null) {
                Debug.LogError("Companion " + companion.companionType.name + " has no speaker type, speaker initialization will fail");
            }
            PrefabInstantiator.InstantiateTeamSelectionCompanion(teamSelectSpeakerPrefab, 
                companion, speakerPositions[i], transform);
            
        }
        yield return StartCoroutine(distributeSequencesToSpeakers());
    }

    private void initializeSpeakerPositions() {
        speakerPositions = new List<Vector2>();
        foreach(Transform speakerLocation in speakerLocations) {
            speakerPositions.Add(speakerLocation.position);
        }
        if(speakerPositions.Count < gameState.companions.companionList.Count) {
            Debug.LogError("Not enough speaker locations for the number of companions, " + 
                "dumping Vector2.identities into the list to make up the difference");
            for(int i = 0; i < gameState.companions.companionList.Count - speakerLocations.Count; i++) {
                speakerPositions.Add(Vector2.zero);
            }
        }
    }

    // wait until the DialogueManager has seen the instantiated dialogueSpeakers
    private IEnumerator distributeSequencesToSpeakers() {
        yield return new WaitUntil(() => DialogueManager.Instance.initialized);
        yield return new WaitUntil(() => 
            DialogueManager.Instance.GetDialogueSpeakersCount() == gameState.companions.companionList.Count);
        List<DialogueSequenceSO> sequencesWithPresentSpeakers = 
            DialogueManager.Instance.GetDialogueSequencesWithPresentSpeakers();
        foreach(DialogueSequenceSO sequence in sequencesWithPresentSpeakers) {
            Debug.Log("got a sequence with present speakers: " + sequence.name);
        }
        

    }


    




}