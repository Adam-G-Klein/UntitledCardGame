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
        bool statusShouldDisplay = false;
        foreach (KeyValuePair<StatusEffect, StatusEffectDisplay> kv in statusEffectDisplays) {
            statusShouldDisplay = entity.statusEffects[kv.Key] != CombatInstance.initialStatusEffects[kv.Key];
            kv.Value.SetDisplaying(statusShouldDisplay);
            if (statusShouldDisplay) kv.Value.SetText(entity.statusEffects[kv.Key].ToString());
        }
    }
}
