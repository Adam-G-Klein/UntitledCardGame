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
    }

    void Update() {
        if (GameplayConstantsSingleton.Instance.gameplayConstants.DEVELOPMENT_MODE && Input.GetKeyDown(KeyCode.M)) {
            currentMana += 1;
        }
    }

    public void manaEventHandler(int info) {
        currentMana += info;
    }

    public void updateMana(int change) {
        currentMana += change;
    }

    public void turnPhaseEventHandler(TurnPhaseEventInfo info) {
        if(info.newPhase == TurnPhase.START_PLAYER_TURN) {
            currentMana = GameplayConstantsSingleton.Instance.gameplayConstants.START_TURN_MANA;
        }
    }

}