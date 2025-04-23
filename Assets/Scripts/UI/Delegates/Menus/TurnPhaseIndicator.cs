using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(TurnPhaseEventListener))]
public class TurnPhaseIndicator : MonoBehaviour
{
    private TextMeshProUGUI turnPhaseText;

    void Start() {
        turnPhaseText = GetComponent<TextMeshProUGUI>();
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
        Debug.Log("Turn phase changed to " + info.newPhase);
        turnPhaseText.text = "Turn phase: " + info.newPhase.ToString();
    }

}