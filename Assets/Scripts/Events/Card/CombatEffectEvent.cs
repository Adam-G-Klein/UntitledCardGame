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
    AddToDamageMultiply,
    ApplyInvulnerability,
    Heal,
    ApplyMaxHpBounty,
    ApplyTemporaryStrength,
    ApplyMinionsOnDeath,
    ApplyPlatedArmor
}

[System.Serializable]
public class CombatEffectEventInfo {
    public CombatEntityInstance effector;
    public Dictionary<CombatEffect, int> combatEffects;
    public List<TargettableEntity> targets;

    public CombatEffectEventInfo(Dictionary<CombatEffect, int> combatEffects, List<TargettableEntity> targets, CombatEntityInstance effector) {
        this.combatEffects = combatEffects;
        this.targets = targets;
        this.effector = effector;
    }

    public CombatEffectEventInfo(EnemyIntent intent) {
        this.targets = intent.targets;
        this.combatEffects = intent.combatEffects;
    }
}

[CreateAssetMenu(
    fileName = "NewCombatEffectEvent", 
    menuName = "Events/Card/Combat Effect Event")]
public class CombatEffectEvent : BaseGameEvent<CombatEffectEventInfo> { 

    public static Dictionary<CombatEffect, StatusEffect> combatEffectToStatusEffect = new Dictionary<CombatEffect, StatusEffect>() {
        {CombatEffect.Weakness, StatusEffect.Weakness},
        {CombatEffect.Strength, StatusEffect.Strength},
        {CombatEffect.Defended, StatusEffect.Defended},
        {CombatEffect.AddToDamageMultiply, StatusEffect.DamageMultiply},
        {CombatEffect.ApplyInvulnerability, StatusEffect.Invulnerability},
        {CombatEffect.ApplyMaxHpBounty, StatusEffect.MaxHpBounty},
        {CombatEffect.ApplyTemporaryStrength, StatusEffect.TemporaryStrength},
        {CombatEffect.ApplyMinionsOnDeath, StatusEffect.MinionsOnDeath},
        {CombatEffect.ApplyPlatedArmor, StatusEffect.PlatedArmor}
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
            // this code is nice enough that I'm willing to work in other places
            // to preserve this: a combat effect's scale signifies what's being added to the 
            // status effect's scale
            statusEffects[combatEffectToStatusEffect[combatEffect]] += scale;
        }
    }
}
