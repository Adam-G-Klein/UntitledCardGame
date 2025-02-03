using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy : Entity, ICombatStats, IUIEntity {
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

    public EnemyIntent ChooseIntent(EnemyInstance enemyInstance) {
        bool belowHalf = enemyInstance.combatInstance.combatStats.currentHealth <= enemyInstance.combatInstance.combatStats.maxHealth / 2;
        if (enemyType.belowHalfHPEnemyPattern != null &&
            enemyType.belowHalfHPEnemyPattern.behaviors != null &&
            enemyType.belowHalfHPEnemyPattern.behaviors.Count > 0 &&
            belowHalf
        ) {
            return enemyType.belowHalfHPEnemyPattern.ChooseIntent(enemyInstance);
        }
        return enemyType.enemyPattern.ChooseIntent(enemyInstance);
    }

    public Sprite getSprite() {
        return this.enemyType.sprite;
    }

    public CombatStats GetCombatStats()
    {
        return this.combatStats;
    }

    public string GetName()
    {
        return this.enemyType.displayName;
    }

    public int GetCurrentHealth()
    {
        return this.combatStats.currentHealth;
    }

    public string GetDescription()
    {
        // intents filled out at runtime or by enemyInstance
        return "";
    }

    public CombatInstance GetCombatInstance()
    {
        return null;
    }

    public EnemyInstance GetEnemyInstance()
    {
        return null;
    }

    public DeckInstance GetDeckInstance()
    {
        return null;
    }
}
