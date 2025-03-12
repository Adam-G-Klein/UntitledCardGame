using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public interface IUIEntity : ICombatStats
{
    public string GetName();
    public int GetCurrentHealth();

    public string GetDescription();

    // will return null if the entity is not in combat, or if combat is still initializing
    public CombatInstance GetCombatInstance(); 

    // will return null if the entity is not an enemy instance
    public EnemyInstance GetEnemyInstance();

    // will return null if not in combat or not a companion
    public DeckInstance GetDeckInstance();
    public Targetable GetTargetable();
    Sprite GetBackgroundImage();
    Sprite GetEntityFrame();
}
