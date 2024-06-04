using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyPortrait : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private EntityHealthBar healthBar;
    [SerializeField] private StatusEffectDisplaysController statusEffectsController;
    [SerializeField] private EnemyIntentDisplay enemyIntentDisplay;
    public EnemyInstance enemyInstance;

    public void Setup(EnemyInstance enemyInstance) {
        Debug.Log("Setting up enemy portrait for " + enemyInstance.name);
        this.enemyInstance = enemyInstance;
        this.healthBar.Setup(enemyInstance.combatInstance);
        this.statusEffectsController.Setup(enemyInstance.combatInstance);
        this.enemyIntentDisplay.Setup(enemyInstance);
        UpdatePortrait();
    }

    // Update is called once per frame
    void Update()
    {
        // This could theoretically be event based in the future
        UpdatePortrait();
    }

    private void UpdatePortrait() {
        this.image.sprite = enemyInstance.enemy.enemyType.sprite;
    }
}
