using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public enum StatusEffectType
{
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
    MoneyOnDeath,
    Charge,
    BonusBlock,
    MaxBlockToLoseAtEndOfTurn,
    Burn
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

    [SerializeField]
    private StatusEffectsSO statusEffectsSO;

    private List<VisualElement> drawnTabs = new List<VisualElement>();


    public void Setup(CombatInstance combatInstance, WorldPositionVisualElement wpve)  {
        Debug.Log("Setting up status effect displays for " + combatInstance.name);
        this.combatInstance = combatInstance;

        statusEffectsParent = wpve.rootElement.Q<VisualElement>(
            className: wpve.portraitContainerName + CombatEncounterView.STATUS_EFFECTS_CONTAINER_SUFFIX
        );

    }

}
