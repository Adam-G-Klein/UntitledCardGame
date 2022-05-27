using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEventBus;
using TMPro;

public class TestTurnInfo : MonoBehaviour
{
    private EventBus eventBus;
    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        eventBus = GameObject.FindGameObjectWithTag("Managers")
            .GetComponent<BattleManager>()
            .getEventBus();
        eventBus.Subscribe<EndPlayerTurnEvent>(onEndPlayerTurnEvent);
        eventBus.Subscribe<StartPlayerTurnEvent>(onStartPlayerTurnEvent);
        text = gameObject.GetComponent<TextMeshProUGUI>();
    }

    private void onEndPlayerTurnEvent(EndPlayerTurnEvent endEvent) {
        text.text = "Enemy Turn";
    }

    private void onStartPlayerTurnEvent(StartPlayerTurnEvent startEvent) {
        text.text = "Player Turn\nClick enemy to deal damage";
    }
}
