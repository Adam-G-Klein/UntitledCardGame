using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip {
    public string plainText;

    public Tooltip(string plainText) {
        this.plainText = plainText;
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