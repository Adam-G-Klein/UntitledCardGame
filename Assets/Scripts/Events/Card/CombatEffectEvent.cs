using System.Collections.Generic;
using UnityEngine;

public enum CombatEffect
{
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
    ApplyPlatedArmor,
    FixedDamageWithCardModifications,
    SetToHalfHealth,
    FixedDamageThatIgnoresBlock,
    Charge,
}

[System.Serializable]
public class CombatEffectEventInfo {
    public CombatInstance effector;
    public Dictionary<CombatEffect, int> combatEffects;
    public List<CombatInstance> targets;

    public CombatEffectEventInfo(
        Dictionary<CombatEffect, int> combatEffects,
        List<CombatInstance> targets,
        CombatInstance effector)
    {
        this.combatEffects = combatEffects;
        this.targets = targets;
        this.effector = effector;
    }

    // public CombatEffectEventInfo(EnemyIntent intent) {
    //     this.targets = intent.targets;
    //     this.combatEffects = intent.combatEffects;
    // }
}

[CreateAssetMenu(
    fileName = "NewCombatEffectEvent",
    menuName = "Events/Card/Combat Effect Event")]
public class CombatEffectEvent : BaseGameEvent<CombatEffectEventInfo> {

    public static Dictionary<CombatEffect, StatusEffectType> combatEffectToStatusEffect = new Dictionary<CombatEffect, StatusEffectType>() {
        {CombatEffect.Weakness, StatusEffectType.Weakness},
        {CombatEffect.Strength, StatusEffectType.Strength},
        {CombatEffect.Defended, StatusEffectType.Defended},
        {CombatEffect.AddToDamageMultiply, StatusEffectType.DamageMultiply},
        {CombatEffect.ApplyInvulnerability, StatusEffectType.Invulnerability},
        {CombatEffect.ApplyMaxHpBounty, StatusEffectType.MaxHpBounty},
        {CombatEffect.ApplyTemporaryStrength, StatusEffectType.TemporaryStrength},
        {CombatEffect.ApplyMinionsOnDeath, StatusEffectType.MinionsOnDeath},
        {CombatEffect.ApplyPlatedArmor, StatusEffectType.PlatedArmor}
    };

    // Applies the status effects in the provided combatEffect dictionary to the statusEffects dictionary
    // yeah this should probably be a non-static method on the eventInfo class but I don't wanna move it
    // and refactor for that rn
    public static void applyCombatEffectStatuses(Dictionary<CombatEffect, int> combatEffects,
        Dictionary<StatusEffectType, int> statusEffects)
    {
        foreach (KeyValuePair<CombatEffect, int> entry in combatEffects) {
            applyCombatEffectStatus(entry.Key, entry.Value, statusEffects);
        }
    }

    public static void applyCombatEffectStatus(CombatEffect combatEffect, int scale,
        Dictionary<StatusEffectType, int> statusEffects)
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
