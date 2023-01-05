using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CardCastEventListener))]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class ManaManager : MonoBehaviour {

    public int currentMana = 3;

    public GameplayConstants constants;
    private TextMeshProUGUI text;

    void Start() {
        text = GetComponentInChildren<TextMeshProUGUI>();
        updateText();
    }

    void Update() {
        if(constants.DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.M)) {
            currentMana += 1;
            updateText();
        }
    }

    public void cardCastEventHandler(CardCastEventInfo info){
        currentMana -= info.cardInfo.cost;
        updateText();
    }

    public void turnPhaseEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.START_PLAYER_TURN) {
            currentMana = constants.START_TURN_MANA;
            updateText();
        }
    }

    private void updateText(){
        text.text = currentMana.ToString();
    }
}