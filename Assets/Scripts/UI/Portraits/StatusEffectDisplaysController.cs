using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectDisplaysController : MonoBehaviour
{
    private CombatInstance entity;
    private Dictionary<StatusEffect, StatusEffectDisplay> statusEffectDisplays = new Dictionary<StatusEffect, StatusEffectDisplay>();

    public void Setup(CombatInstance combatInstance)  {
        Debug.Log("Setting up status effect displays for " + combatInstance.name);

        this.entity = combatInstance;
    }

    void Awake()
    {
        StatusEffectDisplay[] arr = GetComponentsInChildren<StatusEffectDisplay>();

        foreach (StatusEffectDisplay statusEffectDisplay in arr) {
            statusEffectDisplays.Add(statusEffectDisplay.statusEffect, statusEffectDisplay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStatusDisplays();
    }

    private void UpdateStatusDisplays() {
        bool statusShouldDisplay;
        foreach (KeyValuePair<StatusEffect, StatusEffectDisplay> kv in statusEffectDisplays) {
            int statusValue = entity.statusEffects[kv.Key];
            if (kv.Key == StatusEffect.Strength) {
                statusValue += entity.combatStats.baseAttackDamage;
            }
            statusShouldDisplay = statusValue != CombatInstance.initialStatusEffects[kv.Key];
            kv.Value.SetDisplaying(statusShouldDisplay);
            if (statusShouldDisplay) kv.Value.SetText(statusValue.ToString());
        }
    }
}
