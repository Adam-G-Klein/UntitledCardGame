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

    [Header("Screenspace view")]
    [SerializeField] private GameObject screenSpaceView;
    private UIDocument screenSpaceUiDoc;
    private RawImage screenSpaceRawImage;

    [Header("Screenspace layout (percent of screen)")]
    [SerializeField] private float screenSpaceLeftPercent = 0f;
    [SerializeField] private float screenSpaceTopPercent = 0f;
    [SerializeField] private float screenSpaceWidthPercent = 60f;
    [SerializeField] private float screenSpaceHeightPercent = 55f;
    [SerializeField] private float screenSpaceFontScale = 0.7f;

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
    private UIDocument activeUiDoc;
    private RawImage activeRawImage;

    // Start is called before the first frame update
    void Awake()
    {
        if (screenSpaceView != null) {
            screenSpaceUiDoc = screenSpaceView.GetComponentInChildren<UIDocument>(true);
            screenSpaceRawImage = screenSpaceView.GetComponentInChildren<RawImage>(true);
            screenSpaceView.SetActive(false);
        }
        activeUiDoc = uiDoc;
        activeRawImage = rawImage;
        BindActiveDocElements();
        ApplyScreenSpaceLayout();
        if (rawImage != null) rawImage.enabled = false;
        if (screenSpaceRawImage != null) screenSpaceRawImage.enabled = false;
    }

    private void BindActiveDocElements()
    {
        if (activeUiDoc == null) return;
        var root = activeUiDoc.rootVisualElement;
        if (root == null) return;
        portraitElement = root.Q<VisualElement>("speaker-portrait");
        label = root.Q<Label>("main-text-label");
        clickToProceedVE = root.Q<VisualElement>("click-to-continue");
        if (clickToProceedVE != null) clickToProceedVE.style.display = DisplayStyle.None;
    }

    private void ApplyScreenSpaceLayout()
    {
        if (screenSpaceUiDoc == null) return;
        var root = screenSpaceUiDoc.rootVisualElement;
        if (root == null) return;
        root.style.position = Position.Absolute;
        root.style.left = Length.Percent(screenSpaceLeftPercent);
        root.style.top = Length.Percent(screenSpaceTopPercent);
        root.style.width = Length.Percent(screenSpaceWidthPercent);
        root.style.height = Length.Percent(screenSpaceHeightPercent);
        var ssLabel = root.Q<Label>("main-text-label");
        if (ssLabel != null) ssLabel.style.fontSize = Mathf.RoundToInt(maxFontSize * screenSpaceFontScale);
    }

    private void SetActiveViewVisible(bool visible)
    {
        if (activeRawImage != null) activeRawImage.enabled = visible;
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

    public void SetScreenSpaceSortingOrder(string sortingLayer, int sortingOrder) {
        if (screenSpaceRawImage == null) return;
        Canvas ssCanvas = screenSpaceView.GetComponentInChildren<Canvas>(true);
        if (ssCanvas == null) return;
        ssCanvas.overrideSorting = true;
        ssCanvas.sortingLayerName = sortingLayer;
        ssCanvas.sortingOrder = sortingOrder;
    }

    public void SetScreenSpaceMode(bool screenSpace) {
        if (screenSpace == screenSpaceModeActive) return;
        if (activeRawImage != null) activeRawImage.enabled = false;
        if (screenSpace) {
            if (screenSpaceView != null) screenSpaceView.SetActive(true);
            activeUiDoc = screenSpaceUiDoc != null ? screenSpaceUiDoc : uiDoc;
            activeRawImage = screenSpaceRawImage != null ? screenSpaceRawImage : rawImage;
        } else {
            if (screenSpaceView != null) screenSpaceView.SetActive(false);
            activeUiDoc = uiDoc;
            activeRawImage = rawImage;
        }
        screenSpaceModeActive = screenSpace;
        BindActiveDocElements();
        if (screenSpace) ApplyScreenSpaceLayout();
    }

    public void Hide() {
        MusicController.Instance.HandleConciergeDialogue("stop");
        SetActiveViewVisible(false);
    }

    private IEnumerator WaitForClick() {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || hasClicked == true);
    }

    private int CalculateFontSize(string line)
    {
        float percent = (float) line.Count() / (float) maxChars;
        Debug.Log(percent);
        int fontSize = (int)(maxFontSize - (percent * (maxFontSize - minFontSize)));
        if (screenSpaceModeActive) fontSize = Mathf.RoundToInt(fontSize * screenSpaceFontScale);
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
        SetActiveViewVisible(true);
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
        if (hideOnComplete) SetActiveViewVisible(false);
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
