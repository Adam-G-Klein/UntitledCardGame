using System.Collections;
using System.Collections.Generic;
using GameEventBus;
using UnityEngine;

public enum TurnState { 
    Start,
    PlayerTurn,
    PlayerTurnEnd,
    EnemyTurn,
    EnemyTurnEnd,
    Won,
    Lost 
}

public class TurnManager : MonoBehaviour
{
    private EventBus eventBus = new EventBus();
    private TurnState turnState = TurnState.Start;

    public void Start() {
        eventBus.Subscribe<StartPlayerTurnEvent>(onStartPlayerTurnEvent);
        eventBus.Subscribe<EndPlayerTurnEvent>(onEndPlayerTurnEvent);
        eventBus.Subscribe<StartEnemyTurnEvent>(onStartEnemyTurnEvent);
        eventBus.Subscribe<EndEnemyTurnEvent>(onEndEnemyTurnEvent);
        
        eventBus.Publish<StartPlayerTurnEvent>(new StartPlayerTurnEvent());
        turnState = TurnState.PlayerTurn;
    }

    private void onEndPlayerTurnEvent(EndPlayerTurnEvent endPlayerTurnEvent) {
        if (turnState != TurnState.PlayerTurn) {
            Debug.LogError("Can't end player turn while not in player turn");
        }
        turnState = TurnState.PlayerTurnEnd;
        StartCoroutine(WaitForPlayerTurnEnd());
    }

    private IEnumerator WaitForPlayerTurnEnd() {
        yield return new WaitForSeconds(1f);
        eventBus.Publish<StartEnemyTurnEvent>(new StartEnemyTurnEvent());
    }

    private void onStartPlayerTurnEvent(StartPlayerTurnEvent startPlayerTurnEvent) {
        if (turnState != TurnState.EnemyTurnEnd && turnState != TurnState.Start) {
            Debug.LogError("Can't start player turn while not in enemy end turn");
        }
        turnState = TurnState.PlayerTurn;
    } 

    private void onEndEnemyTurnEvent(EndEnemyTurnEvent endEnemyTurnEvent) {
        if (turnState != TurnState.EnemyTurn) {
            Debug.LogError("Can't end enemy turn while not in enemy turn");
        }
        turnState = TurnState.EnemyTurnEnd;
        StartCoroutine(WaitForEnemyTurnEnd());
    }

    private IEnumerator WaitForEnemyTurnEnd() {
        yield return new WaitForSeconds(1f);
        eventBus.Publish<StartPlayerTurnEvent>(new StartPlayerTurnEvent());
    }

    private void onStartEnemyTurnEvent(StartEnemyTurnEvent startEnemyTurnEvent) {
        if (turnState != TurnState.PlayerTurnEnd) {
            Debug.LogError("Can't start enemy turn while not in player end turn");
        }
        turnState = TurnState.EnemyTurn;
    } 

    public EventBus getEventBus() {
        return eventBus;
    }

    public TurnState getBattleState() {
        return turnState;
    }
}