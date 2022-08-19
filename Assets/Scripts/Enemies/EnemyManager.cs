using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemies;
    private EventBus eventBus;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        eventBus = gameObject.GetComponent<TurnManager>().getEventBus();
        eventBus.Subscribe<DamageEvent>(onDamageEvent);
        eventBus.Subscribe<StartEnemyTurnEvent>(onStartEnemyTurnEvent);
    }

    private void onDamageEvent(DamageEvent damageEvent) {
        foreach(Enemy enemy in enemies) {
            if (enemy == damageEvent.getDestinationEntity()) {
                // Multiple by -1 because the damage is 
                // the damage taken as a positive number
                enemy.changeHealth(damageEvent.getDamage() * -1);
            }
        }
    }

    private void onStartEnemyTurnEvent(StartEnemyTurnEvent startEnemyTurnEvent) {
        Debug.Log("Do the enemy turn now");
        StartCoroutine(sleepThenEndEnemyTurn());
    }

    public void setEnemies(List<Enemy> enemies) {
        this.enemies = enemies;
    }

    public void setPlayer(Player player) {
        this.player = player;
    }

    IEnumerator sleepThenEndEnemyTurn() {
        yield return new WaitForSeconds(1.5f);
        eventBus.Publish<EndEnemyTurnEvent>(new EndEnemyTurnEvent());
    }
}
