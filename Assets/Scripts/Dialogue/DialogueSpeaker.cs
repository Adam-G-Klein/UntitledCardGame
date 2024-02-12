using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSpeaker : MonoBehaviour
{

    public SpeakerTypeSO speakerType;
    private DialogueBoxView dialogueBoxView;
    public IEnumerator currentLineCoroutine;
    public bool isSpeaking = false;
    public bool userProceeded = true;

    public bool hasInitiatableDialogue = false;
    public bool initialized = false;
    // Start is called before the first frame update
    void Start()
    {
        if(!initialized) {
            if(speakerType == null) {
                Debug.LogError("Speaker type not set on " + gameObject.name + " DialogueSpeaker");
            }
            InitializeSpeaker(speakerType);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeSpeaker(SpeakerTypeSO speaker) {
        if (initialized) {
            return;
        }
        this.speakerType = speaker;
        Debug.Log("Speaker initialized: " + speakerType);
        dialogueBoxView = GetComponent<DialogueBoxView>();
        dialogueBoxView.InitializeView();
        DialogueManager.Instance.RegisterDialogueSpeaker(this);
        initialized = true;
    }

    public IEnumerator SpeakLine(DialogueLine dialogueLine) {
        if(currentLineCoroutine != null) {
            dialogueBoxView.Clear();
            StopCoroutine(currentLineCoroutine);
        }
        currentLineCoroutine = dialogueBoxView.DisplayDialogue(dialogueLine);
        userProceeded = false;
        StartCoroutine(currentLineCoroutine);
        yield return new WaitUntil(() => userProceeded);
        dialogueBoxView.Clear();
    }

    public void UserButtonClick() {
        if(dialogueBoxView.doneDisplaying) {
            userProceeded = true;
        } else {
            dialogueBoxView.FastForward();
        }
    }
    
}
