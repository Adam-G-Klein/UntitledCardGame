using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// implemented by both companions and enemies
// for the purpose of displaying stats in the UI
public interface CombatEntityBaseStats
{
    int getMaxHealth();
    int getCurrentHealth();
    // For use when leaving combat and persisting companion health
    void setCurrentHealth(int newHealth);
    int getBaseAttackDamage();

    string getId();

    Sprite getSprite();

}
