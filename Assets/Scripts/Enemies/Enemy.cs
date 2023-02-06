using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy: CombatEntityBaseStats {
    public EnemyTypeSO enemyType;
    public int currentHealth;
    public int maxHealth;
    public string id = Id.newGuid();

    public Enemy(EnemyTypeSO enemyType, int initHealth = -1) {
        this.enemyType = enemyType;
        this.maxHealth = enemyType.maxHealth;
        Debug.Log("Enemy.maxHealth: " + enemyType.maxHealth);

        if (initHealth == -1) {
            this.currentHealth = enemyType.maxHealth;
        } else {
            this.currentHealth = initHealth;
        }
    }

    public int getMaxHealth() {
        return this.maxHealth;
    }

    public void setMaxHealth(int newMaxHealth) {
        this.maxHealth = newMaxHealth;
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

    public IEnumerable chooseIntent(EnemyBrainContext context) {
        return enemyType.brain.chooseIntent(context);
    }

    public IEnumerable act(EnemyBrainContext context) {
        return enemyType.brain.act(context);
    }

    public string getId() {
        return this.id;
    }

    public Sprite getSprite() {
        return this.enemyType.sprite;
    }

}
