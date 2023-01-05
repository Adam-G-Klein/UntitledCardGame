using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(UIStateEventListener))]
[RequireComponent(typeof(EffectTargetRequestEventListener))]
public class UIStateIndicator : MonoBehaviour
{
    private TextMeshProUGUI UIStateText;

    void Start() {
        UIStateText = GetComponent<TextMeshProUGUI>();
    }


    public void effectTargetRequestEventHandler(EffectTargetRequestEventInfo info) {
        UIStateText.text = "Getting target, right click to cancel";
    }


    public void UIStateEventHandler(UIStateEventInfo info){
        switch(info.newState) {
            case(UIState.EFFECT_TARGETTING):
                // no op, handled by effect request
                // UIStateText.text = "Getting target for card effect, press 'Q' to cancel";
                break;
            case(UIState.DEFAULT):
                UIStateText.text = "UI in default state for turn phase";
                break;
        }
    }

}