using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System.Linq;
using System.Runtime.InteropServices;

[System.Serializable]
public class TooltipLine {

    public string title;
    public string description;
    public int relatedBehaviorIndex;
    public VisualElement GetVisualElement() {
        VisualElement ve = new VisualElement();
        // below class doesn't exist yet
        // ve.AddToClassList("tooltip-line");

        Label title = new Label(this.title);
        title.AddToClassList("tooltip-title");
        ve.Add(title);

        Label text = new Label(this.description);
        text.AddToClassList("tooltip-text");
        ve.Add(text);

        return ve;
    }

    public TooltipLine(string title, string description, int relatedBehaviorIndex = -1, UnityEngine.UIElements.Image image = null) {
        this.title = title;
        this.description = description;
        this.relatedBehaviorIndex = relatedBehaviorIndex;
    }
}
[System.Serializable]
public class TooltipViewModel {
    public bool empty;
    public List<TooltipLine> lines;
    public string plainText {
        get {
            string text = "";
            foreach(TooltipLine line in lines) {
                text += "*" + line.title + "*\n" + line.description + "\n";
            }
            return text;
        }
    }

    public VisualElement GetVisualElement() {
        VisualElement ve = new VisualElement();
        // below class doesn't exist yet
        // ve.AddToClassList("tooltip-view");
        foreach(TooltipLine line in lines) {
            ve.Add(line.GetVisualElement());
        }
        return ve;
    }

    public TooltipViewModel(string title, string description, int relateBehaviorIndex = -1, UnityEngine.UIElements.Image image = null) {
        this.empty = false;
        this.lines = new List<TooltipLine>();
        lines.Add(new TooltipLine(title, description, relateBehaviorIndex));
    }

    public TooltipViewModel(string plainText) {
        this.empty = false;
        this.lines = new List<TooltipLine>();
        lines.Add(new TooltipLine("", plainText));
    }

    public TooltipViewModel(List<TooltipLine> lines) {
        this.lines = lines;
    }

    public TooltipViewModel(bool empty = true) {
        this.empty = empty;
    }

    // To be expanded upon with images and headers later
    // TODO: prevent duplicate tooltips from getting added together
    // VERY likely to happen if someone adds the strength keyword to a cardtype's
    // tooltipKeyword list when the strength status is already on the card
    public static TooltipViewModel operator +(TooltipViewModel a, TooltipViewModel b) {
        if(a.empty && b.empty) {
            return new TooltipViewModel();
        } else if(a.empty) {
            return b;
        } else if(b.empty) {
            return a;
        }
        a.lines.AddRange(b.lines);
        return new TooltipViewModel(a.lines);
    }
}

[RequireComponent(typeof(MiniUIDocumentWorldspace))]
public class TooltipView : MonoBehaviour
{
    
    public TooltipViewModel tooltip = null;

    [SerializeField]
    [Header("Set below to true to display the tooltipView in the scene at all times.\nUseful for debugging with the prefab manually added to the scene")]
    private bool debugDisplayTooltip = false;

    public VisualElement background;

    void Start() {
        Debug.Log("TooltipView: Start");
        VisualElement root = GetComponent<MiniUIDocumentWorldspace>().doc.rootVisualElement;
        background = root.Q<VisualElement>("tooltip-background");
        Fill();
    }

    public void Fill() {
        background.Add(tooltip.GetVisualElement());
    }

    public void Hide() {
        // TODO: add a quick fade or dissolve effect for funsies?
        Destroy(gameObject);
    }

}