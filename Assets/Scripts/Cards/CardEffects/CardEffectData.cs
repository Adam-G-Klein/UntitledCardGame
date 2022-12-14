using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffectName {
    Draw,
    Damage,
    Buff
}

[Serializable]
public class CardEffectData
{
    
    public CardEffectName effectName;
    public int scale;
    public bool needsTargets;
    [SerializeField]
    public List<EntityType> validTargets;
}
