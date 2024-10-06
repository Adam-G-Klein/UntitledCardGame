using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEntityStatsDisplayViewModel {
    public int currentHealth;
    public int maxHealth;
    public Dictionary<StatusEffectType, string> statusEffects = new Dictionary<StatusEffectType, string>();
    
}
// Handles displaying a companion's stats/providing
// those values to the UI by implementing Entity
public class CombatEntityStatsDisplay: MonoBehaviour
{
    private CombatInstance combatInstance;

    public int maxHealth{
        get {
            return combatInstance.combatStats.maxHealth;
        }
    }

    public int currentHealth{
        get {
            return combatInstance.combatStats.getCurrentHealth();
        }
    }

    private Dictionary<StatusEffectType, CombatInstanceStatusEffectDisplay> statusEffectDisplays = new Dictionary<StatusEffectType, CombatInstanceStatusEffectDisplay>();

    public void Setup(CombatInstance combatInstance) {
        this.combatInstance = combatInstance;   
    }

    public void SetStateDirty() {

    }

    void Awake() {
        CombatInstanceStatusEffectDisplay[] arr = GetComponentsInChildren<CombatInstanceStatusEffectDisplay>();

        foreach (CombatInstanceStatusEffectDisplay statusEffectDisplay in arr) {
            statusEffectDisplays.Add(statusEffectDisplay.statusEffect, statusEffectDisplay);
        }
    }

    void Update() {
        // Should find a way to send a unity event to the children
        // of the CombatEntity prefab every time there's a change to 
        // the stats, so that they can update their own UI
        bool statusShouldDisplay = false;
        foreach (KeyValuePair<StatusEffectType, CombatInstanceStatusEffectDisplay> kv in statusEffectDisplays) {
            statusShouldDisplay = combatInstance.statusEffects[kv.Key] != CombatInstance.initialStatusEffects[kv.Key];
            kv.Value.SetDisplaying(statusShouldDisplay);
            if (statusShouldDisplay) kv.Value.SetText(combatInstance.statusEffects[kv.Key].ToString());
        }
    }
}
