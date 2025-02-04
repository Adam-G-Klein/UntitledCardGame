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
    private EnemyInstance self;
    public List<CompanionInstance> targets;
    public float attackTime;
    public EnemyIntentType intentType;
    public string targetsKey;
    public int displayValue;
    public List<EffectStep> effectSteps;

    public EnemyIntent(
            EnemyInstance self,
            List<CompanionInstance> targets,
            float attackTime,
            EnemyIntentType intentType,
            string targetsKey,
            int displayValue,
            List<EffectStep> effectSteps) {
        this.self = self;
        this.targets = targets;
        this.attackTime = attackTime;
        this.intentType = intentType;
        this.targetsKey = targetsKey;
        this.displayValue = displayValue;
        this.effectSteps = effectSteps;
    }

    public int GetDisplayValue() {
        if (this.intentType != EnemyIntentType.BigAttack && this.intentType != EnemyIntentType.SmallAttack) {
            return this.displayValue;
        }

        return this.displayValue
            + self.combatInstance.combatStats.baseAttackDamage
            + self.combatInstance.GetStatus(StatusEffectType.Strength)
            + self.combatInstance.GetStatus(StatusEffectType.TemporaryStrength);
    }
}
