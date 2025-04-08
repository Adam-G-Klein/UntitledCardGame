using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatStats
{
    public int maxHealth;
    // Keep track of the accumulated max health buffs.
    public int maxHealthBuffs = 0;
    public int currentHealth;
    public int baseAttackDamage;

    public CombatStats(int maxHealth, int baseAttackDamage = 0) {
        Debug.Log("Combat Stats constructor called with maxHealth: " + maxHealth + " and baseAttackDamage: " + baseAttackDamage);
        this.maxHealth = maxHealth;
        this.setCurrentHealth(maxHealth);
        this.baseAttackDamage = baseAttackDamage;
    }

    public CombatStats Clone() {
        return new CombatStats(maxHealth, baseAttackDamage);
    }

    public void MultiplyMaxHealth(float scale) {
        this.maxHealth = (int) (this.maxHealth * scale);
        // Strategically choosing not to set the current health to
        // the new max, if we want to do this whoever called this method
        // should do it.
    }

    public void IncreaseMaxHealth(int scale) {
        this.maxHealth += scale;
        this.maxHealthBuffs += scale;
        // Strategically choosing not to set the current health to
        // the new max, if we want to do this whoever called this method
        // should do it.
    }

    public void IncreaseBaseAttackDamage(int scale) {
        this.baseAttackDamage += scale;
    }

    public int getCurrentHealth() {
        return currentHealth;
    }

    public int getMaxHealth() {
        return maxHealth;
    }

    public int getMaxHealthBuffs() {
        return maxHealthBuffs;
    }

    public void setCurrentHealth(int newHealth) {
        Debug.Log("Setting current health to " + newHealth + " from " + currentHealth);
        this.currentHealth = newHealth;
    }
}
