using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus;

public class TestDealDamageToEnemy : MonoBehaviour
{
    private EventBus eventBus;
    private bool canDealDamage = false;

    // Start is called before the first frame update
    void Start()
    {
        eventBus = GameObject.FindGameObjectWithTag("BattleManager")
            .GetComponent<BattleManager>()
            .getEventBus();
        eventBus.Subscribe<TestAttackingEvent>(onTestAttackingEvent);
        eventBus.Subscribe<TestStopAttackingEvent>(onTestStopAttackingEvent);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canDealDamage) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                TargetableEntity enemy = hit.transform.gameObject.GetComponent<EnemyData>().getEnemy();
                eventBus.Publish<DamageEvent>(new DamageEvent(null, enemy, 1));
                eventBus.Publish<EndPlayerTurnEvent>(new EndPlayerTurnEvent());
                canDealDamage = false;
            }
        }
    }

    void onTestAttackingEvent(TestAttackingEvent testEvent) {
        canDealDamage = true;
    }

    void onTestStopAttackingEvent(TestStopAttackingEvent testEvent) {
        canDealDamage = false;
    }
}
