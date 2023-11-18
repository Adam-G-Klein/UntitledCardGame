using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum StatusEffect {
    Strength,
    Weakness,
    // How we'll implement block for now
    Defended,
    DamageMultiply,
    Invulnerability,
    MaxHpBounty,
    TemporaryStrength,
    // Disabled, see comment in processOnDeathStatusEffects in CombatEntityInstance
    MinionsOnDeath,
    PlatedArmor
}
public class StatusEffectDisplay: MonoBehaviour
{
    
    public bool displaying = false;
    public StatusEffect statusEffect;

    private Vector3 originalScale;
    private TextMeshProUGUI text;

    private CombatEntityStatsDisplay statusEffectsDisplay;
    private RectTransform rectTransform;

    void Awake() {
        originalScale = transform.localScale;
    }

    void Start() {
        hide();
        text = GetComponentInChildren<TextMeshProUGUI>();
        statusEffectsDisplay = GetComponentInParent<CombatEntityStatsDisplay>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void setDisplaying(bool val) {
        if (displaying && !val) {
            hide();
        } else if (!displaying && val) {
            show();
        }
        displaying = val;
    }

    public void setText(string newVal) {
        text.text = newVal;
    }

    private void hide() {
        transform.localScale = Vector3.zero;
    }

    private void show() {
        transform.localScale = originalScale;
        // transform.localPosition = new Vector3(statusEffectsDisplay.getNextStatusXLoc(), 0, 0);
    }

    private void setXLoc(float xLoc) {
        rectTransform.anchoredPosition.Set(xLoc, 0);
    }

}
