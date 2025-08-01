using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    public Enemy(EnemySerializeable enemySerializeable, SORegistry registry) {
        this.enemyType = registry.GetAsset<EnemyTypeSO>(enemySerializeable.enemyTypeGuid);
        this.combatStats = enemySerializeable.combatStats;
        this.entityType = EntityType.Enemy;
        this.id = enemySerializeable.entityId;
    }

    public EnemyIntent ChooseIntent(EnemyInstance enemyInstance) {
        if (enemyInstance.enemy.enemyType.morale == EnemyMorale.AdaptWhenAlone) {
            List<EnemyInstance> allEnemies = CombatEntityManager.Instance.getEnemies();

            int unrelentingCount = 0;
            for (int i = 0; i < allEnemies.Count; i++) {
                if (allEnemies[i].enemy.enemyType.morale == EnemyMorale.Unrelenting) {
                    unrelentingCount += 1;
                }
            }
            if (unrelentingCount == 0) {
                Debug.Log("No more unrelenting friends left. Time for the fallback plan.");
                return enemyInstance.ChooseIntent(enemyType.adaptWhenAloneEnemyPattern);
            }
        }

        bool belowHalf = enemyInstance.combatInstance.combatStats.currentHealth <= enemyInstance.combatInstance.combatStats.maxHealth / 2;
        if (enemyType.belowHalfHPEnemyPattern != null &&
            enemyType.belowHalfHPEnemyPattern.behaviors != null &&
            enemyType.belowHalfHPEnemyPattern.behaviors.Count > 0 &&
            belowHalf
        ) {
            return enemyInstance.ChooseIntent(enemyType.belowHalfHPEnemyPattern);
        }
        return enemyInstance.ChooseIntent(enemyType.enemyPattern);
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

    public Targetable GetTargetable() {
        return null;
    }

    public Sprite GetBackgroundImage() {
        return this.enemyType.backgroundImage;
    }

    public Sprite GetEntityFrame() {
        return this.enemyType.entityFrame;
    }
}

[System.Serializable]
public class EnemySerializeable {
    public string enemyTypeGuid;
    public CombatStats combatStats;
    public string entityId;

    public EnemySerializeable(Enemy enemy) {
        this.enemyTypeGuid = enemy.enemyType.GUID;
        this.combatStats = enemy.combatStats;
        this.entityId = enemy.id;
    }
}