using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// implemented by both companions and enemies
// for the purpose of displaying stats in the UI
public interface CombatEntityBaseStats
{
    int getMaxHealth();
    int getBaseAttackDamage();

    string getId();

    EntityType getEntityType();
    
}
