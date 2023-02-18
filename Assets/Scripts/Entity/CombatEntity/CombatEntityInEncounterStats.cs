using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the math around in-encounter status effects
// affecting a companion or enemy's base stats
// Accessor methods here should be used for all of their stats

// The combatEntityStatsDisplay class will have a reference to this
// for the entity it's displaying

// If you have a better name for this class, please change it
// "Entity" didn't feel specific enough to the fact that we're
// displaying a bunch of different combat related stats
// and we also have the base stats class to differentiate from

public class CombatEntityInEncounterStats  
{
    // The persistent values from the companion itself
    public CombatEntityBaseStats baseStats {set; private get;}

    // The encounter-specific values that 
    // this class is solely responsible for tracking
    public static Dictionary<StatusEffect, int> initialStatusEffects = new Dictionary<StatusEffect, int>(){
        {StatusEffect.Strength, 0},
        {StatusEffect.Weakness, 0},
        {StatusEffect.Defended, 0},
        {StatusEffect.DamageMultiply, 1},
        {StatusEffect.Invulnerability, 0},
        {StatusEffect.MaxHpBounty, 0},
        {StatusEffect.TemporaryStrength, 0},
        {StatusEffect.MinionsOnDeath, 0},
    };

    public Dictionary<StatusEffect, int> statusEffects = new Dictionary<StatusEffect, int>(initialStatusEffects);

    public int currentHealth {
        get {
            return baseStats.getCurrentHealth();
        }
        set {
            baseStats.setCurrentHealth(value);
        }
    }

    public int currentAttackDamage {
        get {
            return (baseStats.getBaseAttackDamage() 
                + statusEffects[StatusEffect.Strength] 
                + statusEffects[StatusEffect.TemporaryStrength]
                - statusEffects[StatusEffect.Weakness]);
        }
    }

    public int maxHealth {
        get {
            return baseStats.getMaxHealth();
        }
    }

    public CombatEntityInEncounterStats(CombatEntityBaseStats entity) {
        this.baseStats = entity;
        // Change this line if we want health to persist between encounters
        // Easier for testing if it resets every time for now
        this.currentHealth = this.maxHealth;
    }

    // takes the base damage of an effect,
    // does math on it based on statuses, and returns the damage that should be done
    public int getEffectScale(CombatEffect effect, int baseScale) {
        switch(effect) {
            case CombatEffect.Damage:
                // use the getter from the stats object
                return (baseScale + currentAttackDamage) 
                    * statusEffects[StatusEffect.DamageMultiply];
            default:
                return baseScale;
        }
    }
    public int getEffectScale(DisplayedCombatEffect effect, int baseScale) {
        switch(effect) {
            case DisplayedCombatEffect.Damage:
                // use the getter from the stats object
                return (baseScale + currentAttackDamage) 
                    * statusEffects[StatusEffect.DamageMultiply];
            default:
                return baseScale;
        }
    }

    public int getDamage(int baseDamage) {
        return getEffectScale(CombatEffect.Damage, baseDamage);
    }
}
