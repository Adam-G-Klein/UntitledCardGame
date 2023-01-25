using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BigAttackBehavior: EnemyBehavior {
    public int damageMultiplier = 2;
    public BigAttackBehavior() {
        enemyBehaviorClassName = "BigAttackBehavior";
    }

    public override EnemyIntent getIntent(EnemyBrainContext context) {
        return new EnemyIntent(new List<TargettableEntity>() {getRandomTarget(context)},
            context.enemyInstance.stats.currentAttackDamage * damageMultiplier, 
            0.2f, 
            new Dictionary<StatusEffect, int>(),
            EnemyIntentType.BigAttack);
    }
}
