using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
public class CardEffectData
{
    
    public SimpleEffectName effectName;
    public int scale;
    public bool needsTargets;
    [SerializeField]
    public List<EntityType> validTargets;
}
