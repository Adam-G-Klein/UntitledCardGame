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

    public Dictionary<StatusEffect, int> statusEffects = new Dictionary<StatusEffect, int>(){
        {StatusEffect.Strength, 0},
        {StatusEffect.Weakness, 0},
        {StatusEffect.Defended, 0}
    };
    
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
            return baseStats.getBaseAttackDamage() 
                + statusEffects[StatusEffect.Strength] 
                - statusEffects[StatusEffect.Weakness];
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

}
