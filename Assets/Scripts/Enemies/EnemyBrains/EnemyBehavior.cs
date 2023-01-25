using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class EnemyBehavior {

    public String enemyBehaviorClassName;
    public EnemyIntent intent;

    protected virtual List<TargettableEntity> getTargets(EnemyBrainContext context) {
        // worth noting that this doesn't work if we want the enemy to attack the whole squad.
        // that enemy behavior will have to make its own list and deliberately ignore taunts
        // Also, probably as expected, taunts won't affect the enemy deciding to buff its team members
        return context.companionManager.getEnemyTargets();
    }

    protected virtual TargettableEntity getRandomTarget(EnemyBrainContext context) {
        List<TargettableEntity> possibleTargets = getTargets(context);
        return possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
    }

    public virtual EnemyIntent getIntent(EnemyBrainContext context) {
        // gross gross just trying to get iteration 1 done with a default enemy behavior
        return new EnemyIntent(new List<TargettableEntity>() {getRandomTarget(context)},
            context.enemyInstance.stats.currentAttackDamage, 
            0.2f, 
            new Dictionary<StatusEffect, int>() {
            },
            EnemyIntentType.SmallAttack);
    }

}
