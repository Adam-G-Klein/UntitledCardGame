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
    
    // Start is called before the first frame update
    void Start()
    {
        if (enemyInstance == null) enemyInstance = GetComponent<EnemyInstance>();
        enemyInstance.combatInstance.updateViewHandler += UpdateViewHandler;

        this.nameLabel = healthBarUIDoc.rootVisualElement.Q<Label>("boss-name-label");
        this.healthBarFill = healthBarUIDoc.rootVisualElement.Q<VisualElement>("boss-health-bar-fill");
        this.healthBarLabel = healthBarUIDoc.rootVisualElement.Q<Label>("boss-health-bar-label");

        UpdateViewHandler();
        
        healthBarUIDoc.rootVisualElement.visible = false;
    }

    public void ShowView() {
        healthBarUIDoc.rootVisualElement.visible = true;
    }

    private void UpdateViewHandler() {
        if (isHealthTweening) return;

        int currentHealth = this.enemyInstance.combatInstance.combatStats.currentHealth;
        int maxHealth = this.enemyInstance.combatInstance.combatStats.maxHealth;

        if (currentHealth == lastHealthValue) return;

        isHealthTweening = true;

        float pointsPerSecond = 8f;
        int healthDifference = lastHealthValue - currentHealth;
        LeanTween.value(lastHealthValue, currentHealth, healthDifference / pointsPerSecond)
            .setEase(LeanTweenType.linear)
            .setOnUpdate((float val) => {
                int intVal = Mathf.RoundToInt(val);
                this.healthBarLabel.text = String.Format(HEALTH_LABEL_STRING, intVal, maxHealth);
                float healthPercent = val / (float) maxHealth;
                this.healthBarFill.style.width = Length.Percent(healthPercent * 100);
            })
            .setOnComplete(() => {
                isHealthTweening = false;
                lastHealthValue = currentHealth;
                // In case multiple instances of damage come through in close timing
                UpdateViewHandler();
            });
    }
}
