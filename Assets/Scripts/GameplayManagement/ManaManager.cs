using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TurnPhaseEventListener))]
public class ManaManager : GenericSingleton<ManaManager> {

    public int currentMana = 3;
    private TextMeshProUGUI text;

    void Start() {
        text = GetComponentInChildren<TextMeshProUGUI>();
        updateText();
    }

    void Update() {
        if (GameplayConstantsSingleton.Instance.gameplayConstants.DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.M)) {
            currentMana += 1;
            updateText();
        }
    }

    public void manaEventHandler(int info) {
        currentMana += info;
        updateText();
    }

    public void updateMana(int change) {
        currentMana += change;
        updateText();
    }

    public void turnPhaseEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.START_PLAYER_TURN) {
            currentMana = GameplayConstantsSingleton.Instance.gameplayConstants.START_TURN_MANA;
            updateText();
        }
    }

    private void updateText(){
        text.text = currentMana.ToString();
    }
}