using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemies;
    private EventBus eventBus;

    // Start is called before the first frame update
    void Start()
    {
        eventBus = gameObject.GetComponent<BattleManager>().getEventBus();
        eventBus.Subscribe<DamageEvent>(onDamageEvent);
        eventBus.Subscribe<EndPlayerTurnEvent>(onEndPlayerTurnEvent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void onDamageEvent(DamageEvent damageEvent) {
        foreach(Enemy enemy in enemies) {
            if (enemy == damageEvent.getDestinationEntity()) {
                enemy.changeHealth(damageEvent.getDamage() * -1);
            }
        }
    }

    private void onEndPlayerTurnEvent(EndPlayerTurnEvent endPlayerTurnEvent) {
        Debug.Log("Do the enemy turn now");
        StartCoroutine(sleepThenStartPlayerTurn());
    }

    public void setEnemies(List<Enemy> enemies) {
        this.enemies = enemies;
    }

    IEnumerator sleepThenStartPlayerTurn() {
        yield return new WaitForSeconds(2);
        eventBus.Publish<StartPlayerTurnEvent>(new StartPlayerTurnEvent());
    }

     //boiler plate singleton code
    private static EnemyManager instance;
    void Awake()
    {
        // If the instance reference has not been set yet, 
        if (instance == null)
        {
            // Set this instance as the instance reference.
            instance = this;
        }
        else if(instance != this)
        {
            // If the instance reference has already been set, and this is not the
            // the instance reference, destroy this game object.
            Destroy(gameObject);
        }

        // Do not destroy this object when we load a new scene
        DontDestroyOnLoad(gameObject);
    }
}
