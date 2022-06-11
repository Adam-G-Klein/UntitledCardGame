using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameEventBus;

public class TestAttckButton : MonoBehaviour
{
    public Button button;
    public Color attackingColor;
    public Color notAttackingColor;
    public Color cantAttackColor;

    private EventBus eventBus;
    private bool attacking = false;
    private bool canAttack = true;

    // Start is called before the first frame update
    void Start()
    {
        eventBus = GameObject.FindGameObjectWithTag("BattleManager")
            .GetComponent<BattleManager>()
            .getEventBus();
        eventBus.Subscribe<StartPlayerTurnEvent>(onStartPlayerTurnEvent);
        eventBus.Subscribe<EndPlayerTurnEvent>(onEndPlayerTurnEvent);
    }

    public void attackButtonClicked() {
        if (canAttack) {
            attacking = !attacking;
            if (attacking) {
                button.GetComponent<Image>().color = attackingColor;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Cancel";
                eventBus.Publish<TestAttackingEvent>(new TestAttackingEvent());
            } else {
                button.GetComponent<Image>().color = notAttackingColor;
                button.GetComponentInChildren<TextMeshProUGUI>().text = "Attack";
                eventBus.Publish<TestStopAttackingEvent>(new TestStopAttackingEvent());
            }
        }
    }

    void onStartPlayerTurnEvent(StartPlayerTurnEvent startEvent) {
        canAttack = true;
        button.GetComponent<Image>().color = notAttackingColor;
        button.GetComponentInChildren<TextMeshProUGUI>().text = "Attack";
    }

    void onEndPlayerTurnEvent(EndPlayerTurnEvent endEvent) {
        canAttack = false;
        attacking = false;
        button.GetComponent<Image>().color = cantAttackColor;
        button.GetComponentInChildren<TextMeshProUGUI>().text = "Attack";
    }
}
