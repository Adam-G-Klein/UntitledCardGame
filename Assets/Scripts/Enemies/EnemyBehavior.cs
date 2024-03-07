using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyBehavior {
    public EnemyIntentType intent;
    public EnemyTargetMethod enemyTargetMethod = EnemyTargetMethod.FirstCompanion;
    public string targetsKey = "";
    [SerializeReference]
    public List<EffectStep> effectSteps;
}

public enum EnemyTargetMethod {
    FirstCompanion,
    LastCompanion,
    RandomEnemyNotSelf,
    RandomCompanion,
    LowestHealth
}