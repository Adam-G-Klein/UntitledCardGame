using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy: CombatEntityBaseStats {
    public EnemyTypeSO enemyType;
    public int currentHealth;
    public string id = Id.newGuid();

    public Enemy(EnemyTypeSO enemyType, int maxHealth = -1) {
        this.enemyType = enemyType;

        if (maxHealth == -1) {
            this.currentHealth = enemyType.maxHealth;
        } else {
            this.currentHealth = maxHealth;
        }
    }

    public int getMaxHealth() {
        return enemyType.maxHealth;
    }

    public int getBaseAttackDamage() {
        return enemyType.baseAttackDamage;
    }
}
