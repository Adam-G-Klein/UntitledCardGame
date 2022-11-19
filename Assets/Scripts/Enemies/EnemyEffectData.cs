using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyEffectName {
    Damage,
    Buff
}

[Serializable]
public class EnemyEffectData
{
    
    public EnemyEffectName effectName;
    public int scale;
}
