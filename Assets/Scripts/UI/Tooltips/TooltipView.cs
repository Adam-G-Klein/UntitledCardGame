using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Runtime.InteropServices;

[System.Serializable]
public class TooltipLine {

    public string title;
    public string description;
    public Image image;

    public TooltipLine(string title, string description, Image image = null) {
        this.title = title;
        this.description = description;
        this.image = image;
    }
}
[System.Serializable]
public class Tooltip {
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

    public Tooltip(string title, string description, Image image = null) {
        this.empty = false;
        this.lines = new List<TooltipLine>();
        lines.Add(new TooltipLine(title, description));
    }

    public Tooltip(string plainText) {
        this.empty = false;
        this.lines = new List<TooltipLine>();
        lines.Add(new TooltipLine("", plainText));
    }

    public Tooltip(List<TooltipLine> lines) {
        this.lines = lines;
    }

    public Tooltip(bool empty = true) {
        this.empty = empty;
    }

    // To be expanded upon with images and headers later
    // TODO: prevent duplicate tooltips from getting added together
    // VERY likely to happen if someone adds the strength keyword to a cardtype's
    // tooltipKeyword list when the strength status is already on the card
    public static Tooltip operator +(Tooltip a, Tooltip b) {
        if(a.empty && b.empty) {
            return new Tooltip();
        } else if(a.empty) {
            return b;
        } else if(b.empty) {
            return a;
        }
        a.lines.AddRange(b.lines);
        return new Tooltip(a.lines);
    }
}

public class TooltipView : MonoBehaviour
{
    public TextMeshProUGUI text;

    public Tooltip tooltip = null;

    void Start() {
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = tooltip.plainText;
        // hack to get tooltips in front, temporary until ui doc rework
        Canvas canvas = GetComponentInChildren<Canvas>(); 
        canvas.overrideSorting = true;
        canvas.sortingOrder = 25;
    }

    public void Hide() {
        // TODO: add a quick fade or dissolve effect for funsies?
        Destroy(gameObject);
    }

}