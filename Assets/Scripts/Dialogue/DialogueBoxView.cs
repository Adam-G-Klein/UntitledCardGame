using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;


public class DialogueBoxView : MonoBehaviour
{

    private TextMeshProUGUI text;
    public bool doneDisplaying = false;
    public float charDelay = 0.1f;
    private IEnumerator displayingCoroutine;
    // For re-displaying the prompt after the dialogue has been completed
    // We only know the user's proceeded when the dialogue box is cleared again 
    private Action redisplayPromptCallback = null;

    [Header("Image assets (hi Kalila!)")]
    [SerializeField]

    private GameObject portrait;
    [SerializeField]
    private GameObject dialogueBoxBackground;
    [SerializeField]
    private GameObject textGameObject;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeView() {
        if(dialogueBoxBackground == null) {
            Debug.LogError("DialogueBoxView: dialogueBoxBackground not set, gameobject: " + gameObject.name);
        }
        if(portrait == null) {
            Debug.LogError("DialogueBoxView: portrait not set, gameobject: " + gameObject.name);
        }
        if(textGameObject == null) {
            Debug.LogError("DialogueBoxView: text not set, gameobject: " + gameObject.name);
        }
        text = textGameObject.GetComponent<TextMeshProUGUI>();
        Clear();
    }

    public IEnumerator DisplayDialogue(DialogueLine dialogueLine, Action redisplayPromptCallback = null)
    {
        Debug.Log("Displaying dialogue: " + dialogueLine.line);
        this.redisplayPromptCallback = redisplayPromptCallback;
        SetGameObjectsEnabled(true);
        doneDisplaying = false;
        displayingCoroutine = DisplayText(dialogueLine.line);
        StartCoroutine(displayingCoroutine);
        yield return new WaitUntil(() => doneDisplaying);
        text.text = dialogueLine.line;
    }

    public void FastForward() {
        StopCoroutine(displayingCoroutine);
        doneDisplaying = true;
    }

    public void Clear() {
        StopAllCoroutines();
        SetGameObjectsEnabled(false);
        text.text = "";
        doneDisplaying = true;
        if(redisplayPromptCallback != null) {
            redisplayPromptCallback.Invoke();
        }
    }

    private IEnumerator DisplayText(string textToDisplay) {
        int currentChar = 0;
        while(currentChar < textToDisplay.Length) {
            text.text += textToDisplay[currentChar];
            currentChar++;
            yield return new WaitForSeconds(charDelay);
        }
        doneDisplaying = true;

    }

    private void SetGameObjectsEnabled(bool enabled) {
        Debug.Log("Setting images enabled: " + enabled + " gameobject: " + gameObject.name);
        dialogueBoxBackground.SetActive(enabled);
        portrait.SetActive(enabled);
        textGameObject.SetActive(enabled);
    }
    
}
