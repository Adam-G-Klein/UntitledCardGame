using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyBehavior {
    public EnemyIntentType intent;
    public EnemyTargetMethod enemyTargetMethod = EnemyTargetMethod.FirstCompanion;
    public string targetsKey = "";
    public int displayValue = 0;
    [SerializeReference]
    public List<EffectStep> effectSteps;
}

public enum EnemyTargetMethod {
    FirstCompanion,
    SecondFromFront,
    ThirdFromFront,
    LastCompanion,
    // RandomEnemyNotSelf,
    RandomCompanion,
    LowestHealth,
    AllCompanions,
    None
}