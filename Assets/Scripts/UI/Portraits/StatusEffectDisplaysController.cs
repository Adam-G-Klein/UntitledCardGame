using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectDisplaysController : MonoBehaviour
{
    private CombatInstance entity;
    private Dictionary<StatusEffectType, CombatInstanceStatusEffectDisplay> statusEffectDisplays = new Dictionary<StatusEffectType, CombatInstanceStatusEffectDisplay>();

    public void Setup(CombatInstance combatInstance)  {
        Debug.Log("Setting up status effect displays for " + combatInstance.name);

        this.entity = combatInstance;
    }

    void Awake()
    {
        CombatInstanceStatusEffectDisplay[] arr = GetComponentsInChildren<CombatInstanceStatusEffectDisplay>();

        foreach (CombatInstanceStatusEffectDisplay statusEffectDisplay in arr) {
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
        foreach (KeyValuePair<StatusEffectType, CombatInstanceStatusEffectDisplay> kv in statusEffectDisplays) {
            int statusValue = entity.statusEffects[kv.Key];
            if (kv.Key == StatusEffectType.Strength) {
                statusValue += entity.combatStats.baseAttackDamage;
            }
            statusShouldDisplay = statusValue != CombatInstance.initialStatusEffects[kv.Key];
            kv.Value.SetDisplaying(statusShouldDisplay);
            if (statusShouldDisplay) kv.Value.SetText(statusValue.ToString());
        }
    }
}
