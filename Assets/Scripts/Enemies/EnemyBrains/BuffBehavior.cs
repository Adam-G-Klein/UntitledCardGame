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
        // there's the taunt case we need to take care of here, because we're not going the 
        // normal target acquisition route
        List<EnemyInstance> possibleTargets = context.enemyManager.getEnemies();
        return new EnemyIntent(new List<TargettableEntity>() {possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)]},
            0.2f, 
            new Dictionary<CombatEffect, int>() {
                {CombatEffect.Strength, 1}
            },
            EnemyIntentType.Buff,
            context.enemyInstance);
    }
}
