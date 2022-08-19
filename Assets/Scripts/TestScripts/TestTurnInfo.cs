using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus;
using TMPro;

public class TestTurnInfo : MonoBehaviour
{
    private TurnManager battleManager;
    private TextMeshProUGUI text;
    private bool updateNormally = true;

    // Start is called before the first frame update
    void Start()
    {
        battleManager = GameObject.FindGameObjectWithTag("BattleManager")
            .GetComponent<TurnManager>();
        battleManager.getEventBus().Subscribe<TestAttackingEvent>(onTestAttackingEvent);
        battleManager.getEventBus().Subscribe<TestStopAttackingEvent>(onTestStopAttackingEvent);
        battleManager.getEventBus().Subscribe<DamageEvent>(onDamageEvent);
        text = gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        if (updateNormally)
            text.text = battleManager.getBattleState().ToString();
    }

    void onTestAttackingEvent(TestAttackingEvent testEvent) {
        updateNormally = false;
        text.text = "Click an enemy to deal damage!";
    }

    void onTestStopAttackingEvent(TestStopAttackingEvent testEvent) {
        updateNormally = true;
    }

    void onDamageEvent(DamageEvent damageEvent) {
        updateNormally = true;
    }
}
