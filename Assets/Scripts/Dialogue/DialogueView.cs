using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DialogueView : GenericSingleton<DialogueView>
{
    public UIDocument uiDoc;
    public RawImage rawImage;
    public float charRevealDelay;
    public float postFullTextDelay;
    public int maxChars;
    public int minFontSize;
    public int maxFontSize;

    private VisualElement portraitElement;
    private Label label;

    private Coroutine runningCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        this.portraitElement = uiDoc.rootVisualElement.Q<VisualElement>("speaker-portrait");
        this.label = uiDoc.rootVisualElement.Q<Label>("main-text-label");
    }

    public void SpeakLine(CompanionTypeSO companion, string line)
    {
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        portraitElement.style.backgroundImage = new StyleBackground(companion.fullSprite);
        label.text = "";
        label.style.fontSize = CalculateFontSize(line);
        runningCoroutine = StartCoroutine(Typewriter(line));
    }

    private int CalculateFontSize(string line)
    {
        float percent = (float) line.Count() / (float) maxChars;
        Debug.Log(percent);
        int fontSize = (int)(maxFontSize - (percent * (maxFontSize - minFontSize)));
        Debug.Log(fontSize);
        return fontSize;
    }

    private IEnumerator Typewriter(string fullText)
    {
        yield return new WaitForSeconds(0.05f);
        rawImage.enabled = true;
        string invisibleText = $"<color=#00000000>{fullText}</color>";
        label.text = invisibleText;
        for (int i = 0; i <= fullText.Length; i++)
        {
            string visible = fullText.Substring(0, i);
            string invisible = $"<color=#00000000>{fullText.Substring(i)}</color>";
            label.text = visible + invisible;
            yield return new WaitForSeconds(charRevealDelay);
        }
        yield return new WaitForSeconds(postFullTextDelay);
        rawImage.enabled = false;
        runningCoroutine = null;
    }
}
