using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Tooltip {
    public bool empty;
    public string plainText;

    public Tooltip(string plainText) {
        this.plainText = plainText;
        this.empty = false;
    }

    public Tooltip(bool empty = true) {
        this.empty = empty;
        this.plainText = "";
    }

    // To be expanded upon with images and headers later
    public static Tooltip operator +(Tooltip a, Tooltip b) {
        if(a.empty && b.empty) {
            return new Tooltip();
        } else if(a.empty) {
            return b;
        } else if(b.empty) {
            return a;
        }
        return new Tooltip(a.plainText + "\n" + b.plainText);
    }
}

public class TooltipView : MonoBehaviour
{
    public TextMeshProUGUI text;

    public Tooltip tooltip = null;

    void Start() {
        if(tooltip == null) {
            Debug.LogError("TooltipView instantiated without a tooltip object!");
            return;
        }
        text.text = tooltip.plainText;
    }

    public void Hide() {
        // TODO: add a quick fade or dissolve effect for funsies?
        Destroy(this.gameObject);
    }

    /*
    void Update() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log("Input.mousePos: " + Input.mousePosition + " screenToWorld: " + mousePos);
        transform.position = new Vector3(mousePos.x, mousePos.y, mousePos.z);
    }
    */


}