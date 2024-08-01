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
    IrishGoodbye,
    // Possible later inclusions
    // Defend,
    // Heal,
    // None
}

public class EnemyIntent {
    public List<CompanionInstance> targets;
    public float attackTime;
    public EnemyIntentType intentType;
    public string targetsKey;
    public int displayValue;
    public List<EffectStep> effectSteps;

    public EnemyIntent(
            List<CompanionInstance> targets,
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