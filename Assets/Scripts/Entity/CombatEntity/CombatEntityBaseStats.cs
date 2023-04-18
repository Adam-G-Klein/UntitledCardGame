using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// implemented by both companions and enemies
// for the purpose of displaying stats in the UI
public interface CombatEntityBaseStats
{
    int getMaxHealth();
    void setMaxHealth(int newMaxHealth);
    int getCurrentHealth();
    // For use when leaving combat and persisting companion health
    void setCurrentHealth(int newHealth);
    int getBaseAttackDamage();
    void setBaseAttackDamage(int newBaseAttackDamage);

    string getId();

    Sprite getSprite();


}
