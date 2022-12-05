using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// implemented by both companionInstance and enemyInstance
public interface CombatEntityInstance
{
    
    CombatEntityInEncounterStats getCombatEntityInEncounterStats();
}
