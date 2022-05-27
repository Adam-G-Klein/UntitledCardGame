using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus;

public class TestDealDamageToEnemy : MonoBehaviour
{
    private EventBus eventBus;
    private bool canDealDamage = true;

    // Start is called before the first frame update
    void Start()
    {
        eventBus = GameObject.FindGameObjectWithTag("Managers")
            .GetComponent<BattleManager>()
            .getEventBus();
        eventBus.Subscribe<EndPlayerTurnEvent>(onEndPlayerTurnEvent);
        eventBus.Subscribe<StartPlayerTurnEvent>(onStartPlayerTurnEvent);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canDealDamage){ // if left button pressed...
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                TargetableEntity enemy = hit.transform.gameObject.GetComponent<EnemyDataStore>().getEnemy();
                eventBus.Publish<DamageEvent>(new DamageEvent(null, enemy, 1));
                eventBus.Publish<EndPlayerTurnEvent>(new EndPlayerTurnEvent());
            }
        }
    }

    private void onEndPlayerTurnEvent(EndPlayerTurnEvent endEvent) {
        canDealDamage = false;
    }

    private void onStartPlayerTurnEvent(StartPlayerTurnEvent startEvent) {
        canDealDamage = true;
    }
}
