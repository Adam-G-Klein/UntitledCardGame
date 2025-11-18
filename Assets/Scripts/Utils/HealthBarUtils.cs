using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Collections;

public static class HealthBarUtils {
    private static string HEALTH_LABEL_STRING = "{0}/{1}";

    public static void SetupHealth(
            int currentHealth,
            int maxHealth,
            VisualElement healthBarFill,
            Label healthBarLabel) {
        healthBarLabel.text = String.Format(HEALTH_LABEL_STRING, currentHealth, maxHealth);
        float healthPercent = (float) currentHealth / (float) maxHealth;
        healthBarFill.style.width = Length.Percent(healthPercent * 100);
    }

    public static void UpdateHealth(
            int lastHealthValue,
            int currentHealth,
            int maxHealth,
            VisualElement healthBarFill,
            Label healthBarLabel,
            Action onComplete) {
        float pointsPerSecond = 8f;
        int healthDifference = lastHealthValue - currentHealth;
        LeanTween.value(lastHealthValue, currentHealth, Mathf.Min(.3f, healthDifference / pointsPerSecond))
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) => {
                int intVal = Mathf.RoundToInt(val);
                healthBarLabel.text = String.Format(HEALTH_LABEL_STRING, intVal, maxHealth);
                float healthPercent = val / (float) maxHealth;
                healthBarFill.style.width = Length.Percent(healthPercent * 100);
            })
            .setOnComplete(() => {
                onComplete?.Invoke();
            });
    }
}