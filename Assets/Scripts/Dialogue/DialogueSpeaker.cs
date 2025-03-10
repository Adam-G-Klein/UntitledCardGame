using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSpeaker : MonoBehaviour
{

    public SpeakerTypeSO speakerType;
    private DialogueBoxView dialogueBoxView;
    public IEnumerator currentLineCoroutine;
    public bool isSpeaking = false;
    public bool userProceeded = true;

    public bool hasInitiatableDialogue = false;
    public bool initialized = false;
    public bool alwaysActive = false;
    private InteractionPromptView interactionPromptView;

    // Start is called before the first frame update
    void Start()
    {
        // For speakers that are always enabled in the scene
        if(!initialized && alwaysActive) {
            if(speakerType == null) {
                Debug.Log("Speaker type not set on " + gameObject.name + " DialogueSpeaker");
            } else {
                Debug.Log("Initialized always active speaker from start: " + speakerType);
                InitializeSpeaker(true, speakerType);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeSpeaker(bool enabled, 
        SpeakerTypeSO speaker = null, 
        // InteractionPrompt for team selection screen companions
        InteractionPromptView interactionPromptView = null) {
        if (initialized) {
            return;
        }
        this.interactionPromptView = interactionPromptView;
        initialized = true;
        dialogueBoxView = GetComponent<DialogueBoxView>();
        dialogueBoxView.InitializeView();
        if(!enabled) {
            dialogueBoxView.Clear();
            return;
        }
        this.speakerType = speaker;
        Debug.Log("Speaker initialized: " + speakerType + " interactionPromptView: " + interactionPromptView);
        DialogueManager.Instance.RegisterDialogueSpeaker(this);
    }

    public IEnumerator SpeakLine(DialogueLine dialogueLine) {
        if(currentLineCoroutine != null) {
            dialogueBoxView.Clear();
            StopCoroutine(currentLineCoroutine);
        }
        if(interactionPromptView != null) {
            interactionPromptView.SetVisible(false);
        }
        currentLineCoroutine = dialogueBoxView.DisplayDialogue(dialogueLine, () => { 
            if(interactionPromptView != null) interactionPromptView.SetVisible(true);
        });
        userProceeded = false;
        StartCoroutine(currentLineCoroutine);
        yield return new WaitUntil(() => userProceeded);
        dialogueBoxView.Clear();
    }

    public IEnumerator SpeakLine(string line, CompanionTypeSO speaker, float timeAfterLine = 1.0f) {
        Debug.Log("Speaking line: " + line);
        if(currentLineCoroutine != null) {
            dialogueBoxView.Clear();
            StopCoroutine(currentLineCoroutine);
        }
        if(interactionPromptView != null) {
            interactionPromptView.SetVisible(false);
        }
        currentLineCoroutine = dialogueBoxView.DisplayDialogue(line, speaker, () => { 
            if(interactionPromptView != null) interactionPromptView.SetVisible(true);
        });
        userProceeded = false;
        StartCoroutine(currentLineCoroutine);
        yield return new WaitForSeconds(timeAfterLine);
        dialogueBoxView.Clear();
    }

    public void SkipLine() {
        StopAllCoroutines();
        dialogueBoxView.Clear();
    }

    public void UserButtonClick() {
        Debug.Log("User clicked, userProceeded: " + userProceeded);
        if(dialogueBoxView.doneDisplaying) {
            userProceeded = true;
        } else {
            dialogueBoxView.FastForward();
        }
    }

}
