using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the math around in-encounter status effects
// affecting a companion's base stats
// Accessor methods here should be used for all of a companion's stats
public class CombatEntityStats 
{
    // The persistent values from the companion itself
    public Companion companion {set; private get;}

    // The encounter-specific values that 
    // this class is solely responsible for tracking
    public int strength;

    public int weakness;
    
    public int currentHealth;

    public int currentAttackDamage {
        get {
            return companion.baseAttackDamage + this.strength - this.weakness;
        }
    }

    public int maxHealth {
        get {
            return companion.maxHealth;
        }
    }

    public CombatEntityStats(Companion companion) {
        this.companion = companion;
        this.strength = 0;
        // Change this if we want health to persist between encounters
        this.currentHealth = companion.maxHealth;
    }

}
