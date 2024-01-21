using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles displaying a companion's stats/providing
// those values to the UI by implementing Entity
public class CombatEntityStatsDisplay: MonoBehaviour
{
    private CombatInstance entity;

    public int maxHealth{
        get {
            return entity.combatStats.maxHealth;
        }
    }

    public int currentHealth{
        get {
            return entity.combatStats.currentHealth;
        }
    }

    private Dictionary<StatusEffect, StatusEffectDisplay> statusEffectDisplays = new Dictionary<StatusEffect, StatusEffectDisplay>();

    void Start()
    {
        // This code assumes we always want to reinitialize the
        // in encounter stats when a new encounter starts
        entity = GetComponentInParent<CombatInstance>();
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
        bool statusShouldDisplay = false;
        foreach (KeyValuePair<StatusEffect, StatusEffectDisplay> kv in statusEffectDisplays) {
            statusShouldDisplay = entity.statusEffects[kv.Key] != CombatInstance.initialStatusEffects[kv.Key];
            kv.Value.SetDisplaying(statusShouldDisplay);
            if (statusShouldDisplay) kv.Value.SetText(entity.statusEffects[kv.Key].ToString());
        }
    }
}
