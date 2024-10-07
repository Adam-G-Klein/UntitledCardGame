using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurnPhaseDisplay : MonoBehaviour {
    
    public GameObject playerTurnPrefab;
    public GameObject enemyTurnPrefab;
    // TODO: make this configurable in game settings, game speed type vibe/beat
    public float turnDisplayDuration = 2.75f;

    private IEnumerable playerTurnTrigger;
    private IEnumerable enemyTurnTrigger;

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
        GameObject playerTurn = Instantiate(playerTurnPrefab, transform); 
        yield return new WaitForSeconds(turnDisplayDuration);
        Destroy(playerTurn);
    }

    public IEnumerable DisplayEnemyTurn() {
        GameObject playerTurn = Instantiate(playerTurnPrefab, transform); 
        yield return new WaitForSeconds(turnDisplayDuration);
        Destroy(playerTurn);
    }
}