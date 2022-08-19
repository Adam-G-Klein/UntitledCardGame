using UnityEngine;

public class Companion : Entity
{
    private string prefabName = "DefaultCompanion";
    private int maxHealth = 15;
    private int currentHealth;

    public Companion() {
        this.currentHealth = this.maxHealth;
    }

    public Companion(
            string prefabName,
            int maxHealth) {
        this.prefabName = prefabName;
        this.maxHealth = maxHealth;
        this.currentHealth = this.maxHealth;
    }

    public string getPrefabName() {
        return prefabName;
    }

    public int getHealth()
    {
        return currentHealth;
    }

    public int getMaxHealth()
    {
        return maxHealth;
    }

    public int changeHealth(int x)
    {
        currentHealth = currentHealth + x;
        return currentHealth;
    }

    public void buildCompanion(GameObject prefab, Vector2 location) {
        CompanionFactory companionFactory = new CompanionFactory();
        companionFactory.generateCompanion(this, prefab, location);
    }
}
