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
    private List<Image> boxAndPortrait; // leaving this until we know the full view
    public bool doneDisplaying = false;
    public float charDelay = 0.1f;
    private IEnumerator displayingCoroutine;
    // For re-displaying the prompt after the dialogue has been completed
    // We only know the user's proceeded when the dialogue box is cleared again 
    private Action redisplayPromptCallback = null;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeView() {
        text = GetComponentInChildren<TextMeshProUGUI>();
        boxAndPortrait = GetComponentsInChildren<Image>().ToList();
        Clear();
    }

    public IEnumerator DisplayDialogue(DialogueLine dialogueLine, Action redisplayPromptCallback = null)
    {
        Debug.Log("Displaying dialogue: " + dialogueLine.line);
        this.redisplayPromptCallback = redisplayPromptCallback;
        SetImagesEnabled(true);
        text.enabled = true;
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
        text.text = "";
        text.enabled = false;
        SetImagesEnabled(false);
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

    private void SetImagesEnabled(bool enabled) {
        boxAndPortrait.ForEach(image => image.enabled = enabled);
    }
    
}
