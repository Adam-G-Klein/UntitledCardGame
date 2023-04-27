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
        return CombatEntityManager.Instance.getEnemyTargets();
    }

    protected virtual TargettableEntity getRandomTarget(EnemyBrainContext context) {
        List<TargettableEntity> possibleTargets = getTargets(context);
        return possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
    }

    public virtual EnemyIntent getIntent(EnemyBrainContext context) {
        int damage = context.enemyInstance.stats.currentAttackDamage;
        return new EnemyIntent(new List<TargettableEntity>() {getRandomTarget(context)},
            0.2f, 
            new Dictionary<CombatEffect, int>() {
                {CombatEffect.Damage, damage}
            },
            EnemyIntentType.SmallAttack,
            context.enemyInstance);
    }

}
