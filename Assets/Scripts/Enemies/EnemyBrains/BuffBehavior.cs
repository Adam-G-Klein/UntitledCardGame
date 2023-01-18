using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffBehavior: EnemyBehavior {
    public BuffBehavior() {
        enemyBehaviorClassName = "BuffBehavior";
    }

    public override EnemyIntent getIntent(EnemyBrainContext context) {
        List<EnemyInstance> possibleTargets = context.enemyManager.getEnemies();
        return new EnemyIntent(new List<TargettableEntity>() {possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)]},
            0, 
            0.2f, 
            new Dictionary<StatusEffect, int>() {
                {StatusEffect.Strength, 1}
            },
            EnemyIntentType.Buff);
    }
}
