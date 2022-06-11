using System.Collections;
using System.Collections.Generic;
using GameEventBus;
using UnityEngine;

public enum BattleState { 
    Start,
    PlayerTurn,
    PlayerTurnEnd,
    EnemyTurn,
    EnemyTurnEnd,
    Won,
    Lost 
}

public class BattleManager : MonoBehaviour
{
    private EventBus eventBus = new EventBus();
    private BattleState battleState = BattleState.Start;

    public void Start() {
        eventBus.Subscribe<StartPlayerTurnEvent>(onStartPlayerTurnEvent);
        eventBus.Subscribe<EndPlayerTurnEvent>(onEndPlayerTurnEvent);
        eventBus.Subscribe<StartEnemyTurnEvent>(onStartEnemyTurnEvent);
        eventBus.Subscribe<EndEnemyTurnEvent>(onEndEnemyTurnEvent);
        
        eventBus.Publish<StartPlayerTurnEvent>(new StartPlayerTurnEvent());
        battleState = BattleState.PlayerTurn;
    }

    private void onEndPlayerTurnEvent(EndPlayerTurnEvent endPlayerTurnEvent) {
        if (battleState != BattleState.PlayerTurn) {
            Debug.LogError("Can't end player turn while not in player turn");
        }
        battleState = BattleState.PlayerTurnEnd;
        StartCoroutine(WaitForPlayerTurnEnd());
    }

    private IEnumerator WaitForPlayerTurnEnd() {
        yield return new WaitForSeconds(1f);
        eventBus.Publish<StartEnemyTurnEvent>(new StartEnemyTurnEvent());
    }

    private void onStartPlayerTurnEvent(StartPlayerTurnEvent startPlayerTurnEvent) {
        if (battleState != BattleState.EnemyTurnEnd && battleState != BattleState.Start) {
            Debug.LogError("Can't start player turn while not in enemy end turn");
        }
        battleState = BattleState.PlayerTurn;
    } 

    private void onEndEnemyTurnEvent(EndEnemyTurnEvent endEnemyTurnEvent) {
        if (battleState != BattleState.EnemyTurn) {
            Debug.LogError("Can't end enemy turn while not in enemy turn");
        }
        battleState = BattleState.EnemyTurnEnd;
        StartCoroutine(WaitForEnemyTurnEnd());
    }

    private IEnumerator WaitForEnemyTurnEnd() {
        yield return new WaitForSeconds(1f);
        eventBus.Publish<StartPlayerTurnEvent>(new StartPlayerTurnEvent());
    }

    private void onStartEnemyTurnEvent(StartEnemyTurnEvent startEnemyTurnEvent) {
        if (battleState != BattleState.PlayerTurnEnd) {
            Debug.LogError("Can't start enemy turn while not in player end turn");
        }
        battleState = BattleState.EnemyTurn;
    } 

    public EventBus getEventBus() {
        return eventBus;
    }

    public BattleState getBattleState() {
        return battleState;
    }
}