using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DebuffBehavior: EnemyBehavior {
    public DebuffBehavior() {
        enemyBehaviorClassName = "DebuffBehavior";
    }

    public override EnemyIntent getIntent(EnemyBrainContext context) {
        return new EnemyIntent(new List<TargettableEntity>() {getRandomTarget(context)},
            0.2f, 
            new Dictionary<CombatEffect, int>() {
                {CombatEffect.Weakness, 1}
            },
            EnemyIntentType.Debuff,
            context.enemyInstance);
    }
}
