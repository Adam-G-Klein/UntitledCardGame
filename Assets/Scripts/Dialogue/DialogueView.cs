using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DialogueView : GenericSingleton<DialogueView>, IControlsReceiver
{
    public UIDocument uiDoc;
    public RawImage rawImage;
    public Canvas canvas;
    public float DefaultCharRevealDelay;
    public float postFullTextDelay;
    public int maxChars;
    public int minFontSize;
    public int maxFontSize;

    private VisualElement portraitElement;
    private Label label;

    private Coroutine runningCoroutine = null;
    private bool waitingForClick = false;
    private bool hasClicked = false;
    private VisualElement clickToProceedVE;

    private bool screenSpaceModeActive = false;
    private RenderMode savedCanvasRenderMode;
    private Vector2 savedAnchorMin, savedAnchorMax, savedPivot, savedAnchoredPos, savedSizeDelta;

    // Start is called before the first frame update
    void Awake()
    {
        this.portraitElement = uiDoc.rootVisualElement.Q<VisualElement>("speaker-portrait");
        this.label = uiDoc.rootVisualElement.Q<Label>("main-text-label");
        this.clickToProceedVE = uiDoc.rootVisualElement.Q<VisualElement>("click-to-continue");
        rawImage.enabled = false;
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
        if (runningCoroutine != null) {
            StopCoroutine(runningCoroutine);
            MusicController.Instance.HandleConciergeDialogue("stop");
        }
        portraitElement.style.backgroundImage = new StyleBackground(sprite);
        label.text = "";
        label.style.fontSize = CalculateFontSize(line);
        runningCoroutine = StartCoroutine(Typewriter(line, postFullTextDelay));
    }

    public IEnumerator SpeakLineCoroutine(Sprite sprite, string line, bool waitForClick = false) {
        if (runningCoroutine != null) {
            StopCoroutine(runningCoroutine);
            MusicController.Instance.HandleConciergeDialogue("stop");
        }
        yield return new WaitUntil(() => portraitElement != null); // for when a dialogue is triggered on awake
        portraitElement.style.backgroundImage = new StyleBackground(sprite);
        label.text = "";

        ParsedLine parsed = ParseLine(line);
        label.style.fontSize = CalculateFontSize(parsed.cleanText);

        runningCoroutine = StartCoroutine(Typewriter(parsed.cleanText, 0f, false, parsed.waitsAtIndex, parsed.speedAtIndex));

        yield return runningCoroutine;

        if (!waitForClick) {
            yield break;
        }
        clickToProceedVE.style.display = DisplayStyle.Flex;
        waitingForClick = true;
        runningCoroutine = StartCoroutine(WaitForClick());
        yield return runningCoroutine;
        clickToProceedVE.style.display = DisplayStyle.None;
        waitingForClick = false;
        hasClicked = false;
    }

    public void SetScreenSpaceMode(bool screenSpace) {
        if (screenSpace == screenSpaceModeActive) return;
        var rt = rawImage.rectTransform;
        if (screenSpace) {
            savedCanvasRenderMode = canvas.renderMode;
            savedAnchorMin = rt.anchorMin;
            savedAnchorMax = rt.anchorMax;
            savedPivot = rt.pivot;
            savedAnchoredPos = rt.anchoredPosition;
            savedSizeDelta = rt.sizeDelta;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(-40f, 40f);
        } else {
            canvas.renderMode = savedCanvasRenderMode;
            rt.anchorMin = savedAnchorMin;
            rt.anchorMax = savedAnchorMax;
            rt.pivot = savedPivot;
            rt.anchoredPosition = savedAnchoredPos;
            rt.sizeDelta = savedSizeDelta;
        }
        screenSpaceModeActive = screenSpace;
    }

    public void Hide() {
        MusicController.Instance.HandleConciergeDialogue("stop");
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

    private struct ParsedLine
    {
        public string cleanText;
        public Dictionary<int, float> waitsAtIndex;  // char index -> wait in seconds
        public Dictionary<int, float> speedAtIndex;  // char index -> charRevealDelay override
    }

    private ParsedLine ParseLine(string line)
    {
        var waits = new Dictionary<int, float>();
        var speeds = new Dictionary<int, float>();
        var clean = new StringBuilder();

        int i = 0;
        while (i < line.Length)
        {
            if (line[i] == '<')
            {
                int close = line.IndexOf('>', i);
                if (close > i)
                {
                    string tag = line.Substring(i + 1, close - i - 1);
                    if (tag.StartsWith("wait ") && float.TryParse(tag.Substring(5), out float ms))
                    {
                        waits[clean.Length] = ms / 1000f;
                        i = close + 1;
                        continue;
                    }
                    if (tag.StartsWith("speed ") && float.TryParse(tag.Substring(6), out float speed))
                    {
                        speeds[clean.Length] = speed / 1000f;
                        i = close + 1;
                        continue;
                    }
                }
            }
            clean.Append(line[i]);
            i++;
        }

        return new ParsedLine { cleanText = clean.ToString(), waitsAtIndex = waits, speedAtIndex = speeds };
    }

    private IEnumerator Typewriter(string fullText, float endDelay, bool hideOnComplete = true,
        Dictionary<int, float> waitsAtIndex = null, Dictionary<int, float> speedAtIndex = null)
    {
        hasClicked = false;
        yield return new WaitForSeconds(0.05f);
        rawImage.enabled = true;
        string invisibleText = $"<color=#00000000>{fullText}</color>";
        label.text = invisibleText;
        float currentCharDelay = DefaultCharRevealDelay;
        for (int i = 0; i <= fullText.Length; i++)
        {
            if (i == 0) MusicController.Instance.HandleConciergeDialogue("start");
            if (speedAtIndex != null && speedAtIndex.TryGetValue(i, out float newSpeed))
                currentCharDelay = newSpeed;

            if (waitsAtIndex != null && waitsAtIndex.TryGetValue(i, out float waitSecs))
                yield return new WaitForSeconds(waitSecs);

            string visible = fullText.Substring(0, i);
            string invisible = $"<color=#00000000>{fullText.Substring(i)}</color>";
            label.text = visible + invisible;
            if(hasClicked)
            {
                MusicController.Instance.HandleConciergeDialogue("stop");
                label.text = fullText;
                hasClicked = false;
                break;
            }
            yield return new WaitForSeconds(currentCharDelay);
        }
        yield return new WaitForSeconds(endDelay);
        if (hideOnComplete) rawImage.enabled = false;
        runningCoroutine = null;
        MusicController.Instance.HandleConciergeDialogue("stop");
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
