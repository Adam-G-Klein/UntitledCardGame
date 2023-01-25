using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SmallAttackBehavior: EnemyBehavior {
    public SmallAttackBehavior() {
        enemyBehaviorClassName = "SmallAttackBehavior";
    }

    public override EnemyIntent getIntent(EnemyBrainContext context) {
        List<TargettableEntity> possibleTargets = context.companionManager.getEnemyTargets();
        return new EnemyIntent(new List<TargettableEntity>() {possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)]},
            context.enemyInstance.stats.currentAttackDamage, 
            0.2f, 
            new Dictionary<StatusEffect, int>(),
            EnemyIntentType.SmallAttack);
    }
}
