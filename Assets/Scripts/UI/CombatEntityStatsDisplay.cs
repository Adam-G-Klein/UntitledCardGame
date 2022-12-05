using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles displaying a companion's stats/providing
// those values to the UI by implementing Entity
public class CombatEntityStatsDisplay: MonoBehaviour
{
    private CombatEntityInEncounterStats stats;

    public int maxHealth{
        get {
            return stats.maxHealth;
        }
    }

    public int currentHealth{
        get {
            return stats.currentHealth;
        }
    }

    public int strength{
        get {
            return stats.strength;
        }
    }

    public int weakness{
        get {
            return stats.weakness;
        }
    }

    void Start()
    {
        // This code assumes we always want to reinitialize the
        // in encounter stats when a new encounter starts
        CombatEntityInstance entity = GetComponentInParent<CombatEntityInstance>();
        this.stats = entity.getCombatEntityInEncounterStats();
    }

}
