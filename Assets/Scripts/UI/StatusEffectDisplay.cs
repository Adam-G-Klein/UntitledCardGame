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
    PlatedArmor,
    Orb
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
        Hide();
        text = GetComponentInChildren<TextMeshProUGUI>();
        statusEffectsDisplay = GetComponentInParent<CombatEntityStatsDisplay>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetDisplaying(bool val) {
        if (displaying && !val) {
            Hide();
        } else if (!displaying && val) {
            Show();
        }
        displaying = val;
    }

    public void SetText(string newVal) {
        text.text = newVal;
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }
}
