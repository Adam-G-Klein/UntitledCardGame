using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurnPhaseDisplay : MonoBehaviour {
    
    public GameObject effectParent;
    public GameObject playerTurnPrefab;
    public GameObject enemyTurnPrefab;
    public GameObject victoryPrefab;
    public GameObject defeatPrefab;
    public float effectZOffset = -100f;
    // TODO: make this configurable in game settings, game speed type vibe/beat
    public float turnDisplayDuration = 0.1f;
    public float victoryDuration = 3f;

    // TODO: move to a setup method that's actually tied to our scene building flow
    // these effects might happen in the wrong order from other effects
    void Start() {
        Setup();
    }

    public void Setup() {
        TurnManager.Instance.addTurnPhaseTrigger(new TurnPhaseTrigger(TurnPhase.START_PLAYER_TURN, DisplayPlayerTurn()));
        TurnManager.Instance.addTurnPhaseTrigger(new TurnPhaseTrigger(TurnPhase.START_ENEMY_TURN, DisplayEnemyTurn()));
    }

    public IEnumerable DisplayPlayerTurn() {
        GameObject playerTurn = Instantiate(playerTurnPrefab, effectParent.transform); 
        yield return new WaitForSeconds(turnDisplayDuration);
        Destroy(playerTurn);
    }

    public IEnumerable DisplayEnemyTurn() {
        GameObject playerTurn = Instantiate(enemyTurnPrefab, effectParent.transform);
        yield return new WaitForSeconds(turnDisplayDuration);
        Destroy(playerTurn);
    }

    public void EndEncounterHandler(EndEncounterEventInfo info) {
        print("EndEncounterHandler called, info.outcome is " + info.outcome);
        if(info.outcome == EncounterOutcome.Victory) {
            StartCoroutine(DisplayVictory());
            return;
        }
        StartCoroutine(DisplayDefeat());
    }

    public IEnumerator DisplayVictory() {
        GameObject victory = Instantiate(victoryPrefab, effectParent.transform); 
        victory.transform.localPosition = new Vector3(0, 0, effectZOffset);
        yield return new WaitForSeconds(victoryDuration);
        Destroy(victory);
    }

    public IEnumerator DisplayDefeat() {
        GameObject defeat = Instantiate(defeatPrefab, effectParent.transform); 
        defeat.transform.localPosition = new Vector3(0, 0, effectZOffset);
        yield return new WaitForSeconds(victoryDuration);
        Destroy(defeat);
    }
}