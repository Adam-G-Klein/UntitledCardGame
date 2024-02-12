using System.Collections;
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
        text.text = "";
        boxAndPortrait = GetComponentsInChildren<Image>().ToList();
        boxAndPortrait.ForEach(image => image.enabled = false);
    }

    public IEnumerator DisplayDialogue(DialogueLine dialogueLine)
    {
        SetImagesEnabled(true);
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
        SetImagesEnabled(false);
        doneDisplaying = true;
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
