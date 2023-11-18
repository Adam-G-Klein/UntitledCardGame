using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy : Entity, ICombatStats {
    public EnemyTypeSO enemyType;

    public CombatStats combatStats;

    public Enemy(EnemyTypeSO enemyType) {
        this.enemyType = enemyType;
        this.id = Id.newGuid();
        this.entityType = EntityType.Enemy;
        this.combatStats = new CombatStats(
            enemyType.maxHealth,
            enemyType.baseAttackDamage);
    }

    public EnemyIntent ChooseIntent() {
        return enemyType.enemyPattern.ChooseIntent();
    }

    public CombatStats GetCombatStats()
    {
        return this.combatStats;
    }
}
