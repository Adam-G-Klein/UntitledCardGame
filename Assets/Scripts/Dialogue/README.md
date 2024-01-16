# Dialogue Docs
## Component-Level Overview
### Data objects
1. SpeakerTypeSO has a CompanionTypeSO and an enum dropdown for Companion, Concierge, or MainCharacter 
2. DialogueLine is serialized by the DialogueSequenceSO, has a SpeakerTypeSO and a string that's spoken
3. DialogueSequenceSO has a list of required speakers and then a list of DialogueLineSOs
    - Created a custom editor for the dialogueSequenceSO so that we can manipulate the dialogueLine list
4. DialogueLocationSO has a list of DialogueSequences. This is what the DialogueManager has a reference to. 
    - A thought: We *could* use this in the demo to hardcode pseudo-state-aware dialogue sequences for different encounters. Would just need some logic to switch out which dialogueLocation we're using in each scene, as the DialogueManager is a singleton.
### Classes
1. DialogueManager:
    - Receives DialogueSpeakers' registrations, called directly on it thanks to it extending GenericSingleton 
    
    It has:
    - StartAnyDialogueSequence(), this is what I use in the demo. It gets the first (by list order) sequence from the manager's DialogueLocationSO that hasn't been run and for which it has the required speakers registered. This kicks off a coroutine which will yield on DialogueSpeaker.SpeakLine(DialogueLine) for each line in the chosen sequence.
    - StartDialogueSequence(DialogueSequenceSO) which *could* be provided specific DialogueSequenceSO's through the Unity cinematic timeline. This is untested.
2. DialogueSpeaker:
    - This component sits on the DialogueSpeaker prefab and interfaces with the DialogueBoxView
    - Has a speakerType, registers itself with the DialogueManager in Start()
    - Has SpeakLine and UserButtonClick, which kick off and fastforward / confirm completion of the currently displayed dialogue, respectively.
3. DialogueBoxView:
    - Just handles displaying the text and portraits/text box for a DialogueSpeaker


Notes / Todo:
- We may need multiple locations for a dialogue line to display on the screen. Could detect if two overlap and push them to universally available screen locations. Seems unnecessary for the demo
