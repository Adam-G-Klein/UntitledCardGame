using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSpeaker : MonoBehaviour
{

    public SpeakerTypeSO speaker;
    private DialogueBoxView dialogueBoxView;
    public IEnumerator currentLineCoroutine;
    public bool isSpeaking = false;
    public bool userProceeded = true;
    // Start is called before the first frame update
    void Start()
    {
        dialogueBoxView = GetComponent<DialogueBoxView>();
        DialogueManager.Instance.RegisterDialogueSpeaker(this);
    }

    // Update is called once per frame
    void Update()
    {
        
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
