using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StatusEffectsDisplay))]
[RequireComponent(typeof(EnemyIntentDisplay))]
public class EnemyPillarUIController : MonoBehaviour
{
    [SerializeField] private EntityHealthViewController healthBar;
    [SerializeField] private StatusEffectsDisplay statusEffectsController;
    [SerializeField] private EnemyIntentDisplay enemyIntentDisplay;
    public EnemyInstance enemyInstance;

    public void Setup(EnemyInstance enemyInstance, WorldPositionVisualElement wpve) {
        Debug.Log("Setting up UI for " + enemyInstance.name);
        this.enemyInstance = enemyInstance;
        // TODO: add this back here
        //this.statusEffectsController.Setup(enemyInstance.combatInstance);
        // 
        enemyIntentDisplay = GetComponent<EnemyIntentDisplay>();
        this.enemyIntentDisplay.Setup(enemyInstance);
    }

}
