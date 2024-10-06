using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StatusEffectsDisplay))]
public class EnemyPillarUIController : MonoBehaviour
{
    [SerializeField] private EntityHealthViewController healthBar;
    [SerializeField] private StatusEffectDisplaysController statusEffectsController;
    public CompanionInstance companionInstance;

    public void Setup(EnemyInstance enemyInstance) {
        Debug.Log("Setting up enemy portrait for " + enemyInstance.name);
        this.enemyInstance = enemyInstance;
        // TODO: add this back here
        //this.statusEffectsController.Setup(enemyInstance.combatInstance);
        // 
        enemyIntentDisplay = GetComponent<EnemyIntentDisplay>();
        this.enemyIntentDisplay.Setup(enemyInstance);
    }

}
