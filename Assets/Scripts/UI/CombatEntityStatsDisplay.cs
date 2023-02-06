using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles displaying a companion's stats/providing
// those values to the UI by implementing Entity
public class CombatEntityStatsDisplay: MonoBehaviour
{
    private CombatEntityInEncounterStats stats;

    public int maxHealth{
        get {
            return stats.maxHealth;
        }
    }

    public int currentHealth{
        get {
            return stats.currentHealth;
        }
    }

    public int strength{
        get {
            return stats.statusEffects[StatusEffect.Strength];
        }
    }

    public int weakness{
        get {
            return stats.statusEffects[StatusEffect.Weakness];
        }
    }

    public int defended{
        get {
            return stats.statusEffects[StatusEffect.Defended];
        }
    }

    public int damageMultiply{
        get {
            return stats.statusEffects[StatusEffect.DamageMultiply];
        }
    }
    private Dictionary<StatusEffect, StatusEffectDisplay> statusEffectDisplays = new Dictionary<StatusEffect, StatusEffectDisplay>();

    void Start()
    {
        // This code assumes we always want to reinitialize the
        // in encounter stats when a new encounter starts
        CombatEntityInstance entity = GetComponentInParent<CombatEntityInstance>();
        this.stats = entity.stats;
    }

    void Awake() {
        StatusEffectDisplay[] arr = GetComponentsInChildren<StatusEffectDisplay>();

        foreach (StatusEffectDisplay statusEffectDisplay in arr) {
            statusEffectDisplays.Add(statusEffectDisplay.statusEffect, statusEffectDisplay);
        }
    }

    void Update() {
        // Should find a way to send a unity event to the children
        // of the CombatEntity prefab every time there's a change to 
        // the stats, so that they can update their own UI
        if(strength != 0) {
            statusEffectDisplays[StatusEffect.Strength].setText(strength.ToString());
            statusEffectDisplays[StatusEffect.Strength].setDisplaying(true);
        } else {
            statusEffectDisplays[StatusEffect.Strength].setDisplaying(false);
        }

        if(weakness != 0) {
            statusEffectDisplays[StatusEffect.Weakness].setText(weakness.ToString());
            statusEffectDisplays[StatusEffect.Weakness].setDisplaying(true);
        } else {
            statusEffectDisplays[StatusEffect.Weakness].setDisplaying(false);
        }

        if(defended != 0) {
            statusEffectDisplays[StatusEffect.Defended].setText(defended.ToString());
            statusEffectDisplays[StatusEffect.Defended].setDisplaying(true);
        } else {
            statusEffectDisplays[StatusEffect.Defended].setDisplaying(false);
        }

        if(damageMultiply != 1) {
            statusEffectDisplays[StatusEffect.DamageMultiply].setText(damageMultiply.ToString());
            statusEffectDisplays[StatusEffect.DamageMultiply].setDisplaying(true);
        } else {
            statusEffectDisplays[StatusEffect.DamageMultiply].setDisplaying(false);
        }

    }

    public float getNextStatusXLoc() {
        float xLoc = 0;
        foreach (KeyValuePair<StatusEffect, StatusEffectDisplay> kv in statusEffectDisplays) {
            if (kv.Value.displaying) {
                xLoc += kv.Value.GetComponent<RectTransform>().rect.width;
            }
        }
        return xLoc;
    }

}
