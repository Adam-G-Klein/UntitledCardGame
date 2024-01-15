# Dialogue Development Planning

1. Create DialogueSequenceSO, has a list of required speakers and then a list of DialogueLineSOs
    - Create a custom editor for the dialogueSequenceSO so that we can manipulate the dialogueLine list
2. Create DialogueLine that will be serialized by the DialogueSequenceSO, has a SpeakerTypeSO and a string
3. Create SpeakerTypeSO, has a CompanionTypeSO and an enum dropdown for Companion, Concierge, or Aiden
4. Create an on-canvas gameobject with DialogueSpeaker component that has a speakerSO field. For now, it takes the DialogueSequenceSO directly 
5. Create a "Next Dialogue" button on the canvas that calls NextDialogue on the DialogueSpeaker
6. DialogueSpeaker plays only that speaker's lines, advancing onclick. When encountering a line not by the speaker, don't display anything.
7. Have each DialogueSpeaker register with the DialogueManager singleton with its speakerSO(multiple speakerSOs of the same type will be ordered in the DialogueManager's speaker list such that only one is called for any dialogueLine of that speakerType)
8. Create a DialogueManager that receives the DialogueSpeaker registrations, adds all DialogueSpeakers into a List\<DialogueSpeaker>. 
    - DialogeManager Validates the speakers. Validates its dialogueSequences.
9. Add a DialogueSequence field to the dialogue manager, have it read through and yield on DisplayDialogueLine on the proper DialogueSpeaker for each point in the sequence.
10. Replace the single DialogueSequence with a List<DialogueSequence> in the dialogue manager, have it choose a dialogueSequence for which we have the required DialogueSpeakers registered, or choose nothing at all.
11. Hook the DialogueManager into UnityCinematics, calling the "DisplayAnyDialogue" method from the cinematic manager. (necessary for opening scene, tutorial, and pre-combat scenes)

Optional:
- Modify the DialogueSpeaker's DisplayDialogueLine to yield on a fade in and fade out animation for the dialogue bubble, configurable in the DialogueLine.
- Modify the DialogueSpeaker's DisplayDialogueLine to print out the text of the dialogue at a configurable pace. Call a callback for each letter printed for sound plugin later.




Notes:
- DialogueSequences should be advanceable through timers or through user interaction (would want a UserDialogueInteractor that listens for input and publishes DialogueSkipEvents) (can descope if only displaying one-off lines)
- How can we use the unity cinematic manager for this?
- To Shannon: 
    - confirming the way optional lines will work
    - no game state for now, we have an arch idea and we're keeping the possibility open for the future
    - no two companions of the same type will speak in one dialogue (would be a confusing UX anyways)