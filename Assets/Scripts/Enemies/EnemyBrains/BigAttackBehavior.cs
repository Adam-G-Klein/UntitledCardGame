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
        int damage = context.enemyInstance.stats.currentAttackDamage * damageMultiplier;
        return new EnemyIntent(new List<TargettableEntity>() {getRandomTarget(context)},
            0.2f, 
            new Dictionary<CombatEffect, int>(){
                {CombatEffect.Damage, damage}
            },
            EnemyIntentType.BigAttack,
            context.enemyInstance);
    }
}
