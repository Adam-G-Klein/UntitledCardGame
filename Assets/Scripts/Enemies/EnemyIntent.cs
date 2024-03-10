using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for displaying the image above the enemy
public enum EnemyIntentType {
    BigAttack,
    SmallAttack,
    Buff,
    Debuff,
    ChargingUp,
    // Possible later inclusions
    // Defend,
    // Heal,
    // None
}

public class EnemyIntent {
    public List<CombatInstance> targets;
    public float attackTime;
    public EnemyIntentType intentType;
    public string targetsKey;
    public int displayValue;
    public List<EffectStep> effectSteps;

    public EnemyIntent(
            List<CombatInstance> targets,
            float attackTime,
            EnemyIntentType intentType,
            string targetsKey,
            int displayValue,
            List<EffectStep> effectSteps) {
        this.targets = targets;
        this.attackTime = attackTime;
        this.intentType = intentType;
        this.targetsKey = targetsKey;
        this.displayValue = displayValue;
        this.effectSteps = effectSteps;
    }
}