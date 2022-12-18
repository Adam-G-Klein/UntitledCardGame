using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

// Simple effects are actioned by 
// listeners to SimpleEffectEvents
// See EnemyInstance and CompanionInstance for their 
// handler functions, where they deal damage to themselves
// and handle buffs/debuffs
public enum SimpleEffectName {
    Draw,
    Damage,
    Buff,
    Unset
}

[Serializable]
public class CardEffectData
{
    
    public SimpleEffectName effectName;
    public int scale;
    public bool needsTargets;
    [SerializeField]
    public List<EntityType> validTargets;
}
