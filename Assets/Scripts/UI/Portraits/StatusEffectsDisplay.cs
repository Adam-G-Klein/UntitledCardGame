using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public enum StatusEffectType {
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
    Orb,
    Thorns,
    MoneyOnDeath
}

public class StatusEffectsDisplayViewModel {
    public Dictionary<StatusEffectType, int> statusEffects;
    public StatusEffectsDisplayViewModel(Dictionary<StatusEffectType, int> statusEffectValues) {
        this.statusEffects = statusEffectValues;
    }
}

public class StatusEffectsDisplay: MonoBehaviour
{
    private CombatInstance combatInstance;
    private VisualElement statusEffectsParent;


    public void Setup(CombatInstance combatInstance, WorldPositionVisualElement wpve)  {
        Debug.Log("Setting up status effect displays for " + combatInstance.name);
        this.combatInstance = combatInstance;

        statusEffectsParent = wpve.rootElement.Q<VisualElement>(
            className: wpve.portraitContainerName + CombatEncounterView.STATUS_EFFECTS_CONTAINER_SUFFIX
        );
        for(int i = 0; i < 3 ; i++) {
            VisualElement statusEffectDisplay = new VisualElement();
            statusEffectDisplay.AddToClassList(CombatEncounterView.STATUS_EFFECTS_TAB_CLASSNAME);
            statusEffectDisplay.BringToFront();
            statusEffectsParent.Add(statusEffectDisplay);
        }
        UIStateManager.Instance.SetUIDocDirty();

    }

    public void UpdateStatusDisplays(StatusEffectsDisplayViewModel viewModel) {
        Dictionary<StatusEffectType, int> statusEffectsToDisplay = GetStatusesToDisplay(viewModel);
    }

    private Dictionary<StatusEffectType, int> GetStatusesToDisplay(StatusEffectsDisplayViewModel viewModel) {
        Dictionary<StatusEffectType, int> statusEffectsToDisplay = new Dictionary<StatusEffectType, int>();
        foreach (KeyValuePair<StatusEffectType, int> kv in viewModel.statusEffects) {
            int statusValue = kv.Value;
            if (kv.Key == StatusEffectType.Strength) {
                statusValue += combatInstance.combatStats.baseAttackDamage;
            }
            if(statusValue != CombatInstance.initialStatusEffects[kv.Key]) {
                statusEffectsToDisplay.Add(kv.Key, statusValue);
            }
        }
        return statusEffectsToDisplay;
    }

}
