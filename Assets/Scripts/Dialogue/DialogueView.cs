using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DialogueView : GenericSingleton<DialogueView>, IControlsReceiver
{
    public UIDocument uiDoc;
    public RawImage rawImage;
    public Canvas canvas;
    public float charRevealDelay;
    public float postFullTextDelay;
    public int maxChars;
    public int minFontSize;
    public int maxFontSize;

    private VisualElement portraitElement;
    private Label label;

    private Coroutine runningCoroutine = null;
    private bool waitingForClick = false;
    private bool hasClicked = false;

    // Start is called before the first frame update
    void Awake()
    {
        this.portraitElement = uiDoc.rootVisualElement.Q<VisualElement>("speaker-portrait");
        this.label = uiDoc.rootVisualElement.Q<Label>("main-text-label");
    }

    void Start() {
        ControlsManager.Instance.RegisterControlsReceiver(this);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            hasClicked = true;
        }
    }

    public void SpeakLine(Sprite sprite, string line)
    {
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        portraitElement.style.backgroundImage = new StyleBackground(sprite);
        label.text = "";
        label.style.fontSize = CalculateFontSize(line);
        runningCoroutine = StartCoroutine(Typewriter(line, postFullTextDelay));
    }

    public IEnumerator SpeakLineCoroutine(Sprite sprite, string line, bool waitForClick = false) {
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        portraitElement.style.backgroundImage = new StyleBackground(sprite);
        label.text = "";
        label.style.fontSize = CalculateFontSize(line);

        runningCoroutine = StartCoroutine(Typewriter(line, 0f, false));

        yield return runningCoroutine;

        if (!waitForClick) {
            yield break;
        }

        waitingForClick = true;
        runningCoroutine = StartCoroutine(WaitForClick());
        yield return runningCoroutine;
        waitingForClick = false;
        hasClicked = false;
    }

    public void Hide() {
        rawImage.enabled = false;
    }

    private IEnumerator WaitForClick() {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || hasClicked == true);
    }

    private int CalculateFontSize(string line)
    {
        float percent = (float) line.Count() / (float) maxChars;
        Debug.Log(percent);
        int fontSize = (int)(maxFontSize - (percent * (maxFontSize - minFontSize)));
        Debug.Log(fontSize);
        return fontSize;
    }

    private IEnumerator Typewriter(string fullText, float endDelay, bool hideOnComplete = true)
    {
        hasClicked = false;
        yield return new WaitForSeconds(0.05f);
        rawImage.enabled = true;
        string invisibleText = $"<color=#00000000>{fullText}</color>";
        label.text = invisibleText;
        for (int i = 0; i <= fullText.Length; i++)
        {
            string visible = fullText.Substring(0, i);
            string invisible = $"<color=#00000000>{fullText.Substring(i)}</color>";
            label.text = visible + invisible;
            if(hasClicked)
            {
                label.text = fullText;
                hasClicked = false;
                break;
            }
            yield return new WaitForSeconds(charRevealDelay);
        }
        yield return new WaitForSeconds(endDelay);
        if (hideOnComplete) rawImage.enabled = false;
        runningCoroutine = null;
        yield return null;
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (action == GFGInputAction.SELECT) {
            hasClicked = true;
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        return;
    }
}
