using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class EnemyBehavior {

    public String enemyBehaviorClassName;
    public EnemyIntent intent;

    public virtual EnemyIntent getIntent(EnemyBrainContext context) {
        List<CompanionInstance> possibleTargets = context.companionManager.getCompanions();
        // gross gross just trying to get iteration 1 done with a default enemy behavior
        return new EnemyIntent(new List<TargettableEntity>() {possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)]},
            context.enemyInstance.getCombatEntityInEncounterStats().currentAttackDamage, 
            0.2f, 
            new Dictionary<StatusEffect, int>() {
                {StatusEffect.Weakness, 1}
            });
    }

}
