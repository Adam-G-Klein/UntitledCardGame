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
    public int strength;

    public int weakness;
    
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
            return baseStats.getBaseAttackDamage() + this.strength - this.weakness;
        }
    }

    public int maxHealth {
        get {
            return baseStats.getMaxHealth();
        }
    }

    public CombatEntityInEncounterStats(CombatEntityBaseStats entity) {
        this.strength = 0;
        this.weakness = 0;
        this.baseStats = entity;
        // Change this line if we want health to persist between encounters
        this.currentHealth = this.maxHealth;
    }

}
