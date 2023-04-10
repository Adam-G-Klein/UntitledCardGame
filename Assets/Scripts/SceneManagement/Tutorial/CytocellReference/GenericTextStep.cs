using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextStepArgs {

    public string text = "unset generic arg :(";    
    public float appearanceTime = 1f;

    public TextStepArgs(string text, float appearanceTime){
        this.text = text;
        this.appearanceTime = appearanceTime;
    }
}

public class GenericTextStep : MonoBehaviour
{
    private TextGroupAlphaControls alphaControls;
    private TextMeshProUGUI textComp;

    void Start() {
        alphaControls = GetComponent<TextGroupAlphaControls>();
        textComp = GetComponent<TextMeshProUGUI>();
    }

    public void displayText(float secs, string text){ StartCoroutine("corout", new TextStepArgs(text, secs)); }

    private IEnumerator corout(TextStepArgs args){
        textComp.SetText(args.text);
        alphaControls.displayAll();
        yield return new WaitForSeconds(args.appearanceTime);
        alphaControls.hideAll();
    }
}