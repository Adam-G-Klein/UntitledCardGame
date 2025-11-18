using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeothraHealthDisplay : MonoBehaviour
{

    [SerializeField] private EnemyInstance enemyInstance;
    [SerializeField] private UIDocument healthBarUIDoc;

    private Label nameLabel;
    private VisualElement healthBarFill;
    private Label healthBarLabel;

    private int lastHealthValue;
    private bool isHealthTweening = false;

    private static string HEALTH_LABEL_STRING = "{0}/{1}";
    public bool isSetup = false;
    
    // Start is called before the first frame update
    public void Setup()
    {
        if (enemyInstance == null) enemyInstance = GetComponent<EnemyInstance>();
        enemyInstance.combatInstance.updateViewHandler += UpdateViewHandler;

        this.nameLabel = healthBarUIDoc.rootVisualElement.Q<Label>("boss-name-label");
        this.healthBarFill = healthBarUIDoc.rootVisualElement.Q<VisualElement>("boss-health-bar-fill");
        this.healthBarLabel = healthBarUIDoc.rootVisualElement.Q<Label>("boss-health-bar-label");

        UpdateViewHandler();
        
        healthBarUIDoc.rootVisualElement.visible = false;
        isSetup = true;
    }

    // called from the timeline when we wanna display it
    public void ShowView() {
        healthBarUIDoc.rootVisualElement.visible = true;
    }

    private void UpdateViewHandler() {
        if (isHealthTweening) return;

        int currentHealth = this.enemyInstance.combatInstance.combatStats.currentHealth;
        int maxHealth = this.enemyInstance.combatInstance.combatStats.maxHealth;

        if (currentHealth == lastHealthValue) return;

        isHealthTweening = true;

        HealthBarUtils.UpdateHealth(lastHealthValue, currentHealth, maxHealth, healthBarFill, healthBarLabel, () => {
            isHealthTweening = false;
            lastHealthValue = currentHealth;
            // In case multiple instances of damage come through in close timing
            UpdateViewHandler();
        });
    }
}
