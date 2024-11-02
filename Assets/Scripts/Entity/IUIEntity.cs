using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIEntity : ICombatStats
{
    public string GetName();
    public int GetCurrentHealth();

    public string GetDescription();

    // will return null if the entity is not in combat, or if combat is still initializing
    public CombatInstance GetCombatInstance(); 

    // will return null if the entity is not an enemy instance
    public EnemyInstance GetEnemyInstance();

}
