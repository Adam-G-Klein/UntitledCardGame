using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatEffect {
    Damage,
    Strength,
    Weakness,
    Defended,
    DrawFrom,
    SetHealth,
    Sacrifice,
    DamageMultiply
}

[System.Serializable]
public class CombatEffectEventInfo {
    public Dictionary<CombatEffect, int> combatEffects;
    public List<TargettableEntity> targets;

    public CombatEffectEventInfo(Dictionary<CombatEffect, int> combatEffects, List<TargettableEntity> targets) {
        this.combatEffects = combatEffects;
        this.targets = targets;
    }

    public CombatEffectEventInfo(EnemyIntent intent) {
        this.targets = intent.targets;
        this.combatEffects = intent.combatEffects;
    }

}
[CreateAssetMenu(
    fileName = "CombatEffectEvent", 
    menuName = "Events/Game Event/CombatEffectEvent")]
public class CombatEffectEvent : BaseGameEvent<CombatEffectEventInfo> { 

    public static Dictionary<CombatEffect, StatusEffect> combatEffectToStatusEffect = new Dictionary<CombatEffect, StatusEffect>() {
        {CombatEffect.Weakness, StatusEffect.Weakness},
        {CombatEffect.Strength, StatusEffect.Strength},
        {CombatEffect.Defended, StatusEffect.Defended},
    };

    // Applies the status effects in the provided combatEffect dictionary to the statusEffects dictionary
    // yeah this should probably be a non-static method on the eventInfo class but I don't wanna move it
    // and refactor for that rn
    public static void applyCombatEffectStatuses(Dictionary<CombatEffect, int> combatEffects, 
        Dictionary<StatusEffect, int> statusEffects) 
    {
        foreach (KeyValuePair<CombatEffect, int> entry in combatEffects) {
            applyCombatEffectStatus(entry.Key, entry.Value, statusEffects);
        }
    }

    public static void applyCombatEffectStatus(CombatEffect combatEffect, int scale, 
        Dictionary<StatusEffect, int> statusEffects) 
    {
        if(!combatEffectToStatusEffect.ContainsKey(combatEffect)) {
            return;
        }
        else {
            statusEffects[combatEffectToStatusEffect[combatEffect]] += scale;
        }
    }
}
