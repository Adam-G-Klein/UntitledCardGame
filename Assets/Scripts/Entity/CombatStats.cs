using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombatStats
{
    public int maxHealth;
    public int currentHealth;
    public int baseAttackDamage;

    public CombatStats(int maxHealth, int baseAttackDamage) {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
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
        // Strategically choosing not to set the current health to
        // the new max, if we want to do this whoever called this method
        // should do it.
    }

    public void IncreaseBaseAttackDamage(int scale) {
        this.baseAttackDamage += scale;
    }
}
