using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy: CombatEntityBaseStats {
    public EnemyTypeSO enemyType;
    public int currentHealth;
    public string id = Id.newGuid();

    public Enemy(EnemyTypeSO enemyType, int initHealth = -1) {
        this.enemyType = enemyType;

        if (initHealth == -1) {
            this.currentHealth = enemyType.maxHealth;
        } else {
            this.currentHealth = initHealth;
        }
    }

    public int getMaxHealth() {
        return enemyType.maxHealth;
    }

    public int getCurrentHealth() {
        return currentHealth;
    }

    public void setCurrentHealth(int newHealth) {
        this.currentHealth = newHealth;
    }

    public int getBaseAttackDamage() {
        return enemyType.baseAttackDamage;
    }

    public EnemyIntent getNewEnemyIntent(List<string> possibleTargets, CombatEntityInEncounterStats selfStats) {
        return enemyType.getNewIntent(possibleTargets, selfStats);
    }

    public string getId() {
        return this.id;
    }

}
