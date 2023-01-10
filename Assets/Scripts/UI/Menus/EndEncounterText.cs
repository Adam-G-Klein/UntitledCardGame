using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(EndEncounterEventListener))]
public class EndEncounterText : MonoBehaviour
{
    private TextMeshProUGUI endEncounterText;

    void Start() {
        endEncounterText = GetComponent<TextMeshProUGUI>();
        endEncounterText.text = "";
    }


    public void endEncounterEventHandler(EndEncounterEventInfo info) {
        switch(info.outcome) {
            case(EncounterOutcome.Victory):
                endEncounterText.text = "Ayyyyyy what's poppin winnerrrrr?!";
                break;
            case(EncounterOutcome.Defeat):
                endEncounterText.text = "dam u kinda lost huh";
                break;
        }
    }
}