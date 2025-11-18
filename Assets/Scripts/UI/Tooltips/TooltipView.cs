using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;

[System.Serializable]
public class TooltipLine
{
    public string title;
    public string description;
    public int relatedBehaviorIndex;
    public VisualElement GetVisualElement()
    {
        VisualElement ve = new VisualElement();
        ve.AddToClassList("tooltip-line");

        Label title = new Label(this.title);
        title.AddToClassList("tooltip-title");
        ve.Add(title);

        Label text = new Label(this.description);
        text.AddToClassList("tooltip-text");
        ve.Add(text);

        return ve;
    }

    /* todo, dynamic sizing for tooltips

    private int getDescFontSize(string desc) {
        if (desc.Length > CARD_DESC_MAX_FULL_SIZE_CHARS) {
            Debug.Log("desc.Length: " + desc.Length + " CARD_DESC_MAX_FULL_SIZE_CHARS: " + CARD_DESC_MAX_FULL_SIZE_CHARS + " CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length: " + (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length) + " CARD_DESC_SIZE: " + CARD_DESC_SIZE + " CARD_DESC_SIZE * (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length): " + (CARD_DESC_SIZE * (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length)) + " (int)(CARD_DESC_SIZE * (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length)): " + (int)(CARD_DESC_SIZE * (CARD_DESC_MAX_FULL_SIZE_CHARS / desc.Length)));
            float textSizeRatio = (float) CARD_DESC_MAX_FULL_SIZE_CHARS / (float) desc.Length;
            double scalingRatio = Math.Pow(textSizeRatio, (float)1/ (float)4);
            return (int)Math.Floor(CARD_DESC_SIZE * scalingRatio);
        }
        return CARD_DESC_SIZE;
    }

    private int getTitleFontSize(string title) {
        if (title.Length > CARD_TITLE_MAX_FULL_SIZE_CHARS) {
            float textSizeRatio = (float) CARD_TITLE_MAX_FULL_SIZE_CHARS / (float) title.Length;
            return (int)Math.Floor(CARD_TITLE_SIZE * textSizeRatio);
        }
        return CARD_TITLE_SIZE;
    }
    */

    public TooltipLine(string title, string description, int relatedBehaviorIndex = -1, UnityEngine.UIElements.Image image = null)
    {
        this.title = title;
        this.description = description;
        this.relatedBehaviorIndex = relatedBehaviorIndex;
    }
}

[System.Serializable]
public class TooltipViewModel
{
    public bool empty;
    public List<TooltipLine> lines;
    public string plainText
    {
        get
        {
            string text = "";
            foreach (TooltipLine line in lines)
            {
                text += "*" + line.title + "*\n" + line.description + "\n";
            }
            return text;
        }
    }

    public VisualElement GetVisualElement()
    {
        VisualElement ve = new VisualElement();
        // below class doesn't exist yet
        // ve.AddToClassList("tooltip-view");
        foreach (TooltipLine line in lines)
        {
            ve.Add(line.GetVisualElement());
        }
        return ve;
    }

    public TooltipViewModel(string title, string description, int relateBehaviorIndex = -1, UnityEngine.UIElements.Image image = null)
    {
        this.empty = false;
        this.lines = new List<TooltipLine>();
        lines.Add(new TooltipLine(title, description, relateBehaviorIndex));
    }

    public TooltipViewModel(string plainText)
    {
        this.empty = false;
        this.lines = new List<TooltipLine>();
        lines.Add(new TooltipLine("", plainText));
    }

    public TooltipViewModel(List<TooltipLine> lines)
    {
        this.lines = lines;
    }

    public TooltipViewModel(bool empty = true)
    {
        this.empty = empty;
    }

    // To be expanded upon with images and headers later
    // TODO: prevent duplicate tooltips from getting added together
    // VERY likely to happen if someone adds the strength keyword to a cardtype's
    // tooltipKeyword list when the strength status is already on the card
    // DO NOT MUTATE THE TOOLTIPVIEWMODELS plz.
    public static TooltipViewModel operator +(TooltipViewModel a, TooltipViewModel b)
    {
        List<TooltipLine> newLines = new();
        if (a.empty && b.empty)
        {
            return new TooltipViewModel();
        }
        if (!a.empty)
        {
            newLines.AddRange(a.lines);
        }
        if (!b.empty)
        {
            var existingTitles = new System.Collections.Generic.HashSet<string>(newLines.Select(l => l.title));
            foreach (var line in b.lines)
            {
                if (!existingTitles.Contains(line.title))
                {
                    newLines.Add(line);
                    existingTitles.Add(line.title);
                }
            }
        }
        return new TooltipViewModel(newLines);
    }

    public static TooltipViewModel operator -(TooltipViewModel a, TooltipViewModel b)
    {
        List<TooltipLine> newLines = new();
        if (a.empty && b.empty)
        {
            return new TooltipViewModel();
        }
        if (!a.empty)
        {
            newLines.AddRange(a.lines);
        }
        if (!b.empty)
        {
            foreach (var line in b.lines) {
                newLines.Remove(line);
            }
        }
        return new TooltipViewModel(newLines);
    }
}

public class TooltipView : MonoBehaviour
{

    public TooltipViewModel tooltip = null;
    public Canvas canvas;

    [SerializeField]
    [Header("Set below to true to display the tooltipView in the scene at all times.\nUseful for debugging with the prefab manually added to the scene")]
    private bool debugDisplayTooltip = false;

    [SerializeField]
    private bool canvasTooltip = false;

    public VisualElement background;
    private Material mat;

    public float fadeInOutTime = 0.25f;
    public float waitForUIDocFillTime = 0.1f;


    void Start()
    {
        Debug.Log("TooltipView: Start");
        VisualElement root = GetComponent<MiniUIDocumentScreenspace>().doc.rootVisualElement;
        background = root.Q<VisualElement>("tooltip-background");
        mat = GetComponent<RawImage>().material;
        canvas.sortingLayerName = "OverlayUI";
        transform.position = new Vector3(transform.position.x, transform.position.y, 15f);
        Fill();
    }

    public void Fill()
    {
        background.Add(tooltip.GetVisualElement());
        UIDocumentUtils.SetAllPickingMode(background, PickingMode.Ignore);
        /* TODO: make tooltips support transparency without getting occluded
            by the combat UI. Timeboxed before magwest
        LeanTween.value(0, 1, fadeInOutTime)
            .setOnUpdate((float val) => {
                mat.SetFloat("_alpha", val);
            });
            */

        Invoke("Display", waitForUIDocFillTime);
    }

    public void Display()
    {
        GetComponent<RawImage>().enabled = true;
    }

    public void Hide()
    {
        // TODO: add a quick fade or dissolve effect for funsies?
        /* TODO: make tooltips support transparency without getting occluded
            by the combat UI. Timeboxed before magwest
        LeanTween.value(1, 0, fadeInOutTime)
            .setOnUpdate((float val) =>
            {
                mat.SetFloat("_alpha", val);
            }).setOnComplete(() =>
            {
                background.Clear();
                Destroy(gameObject);
            });
            */
        background.Clear();
        Destroy(gameObject);
    }

}