using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultEnemy : Enemy
{
    int health = 5;
    int maxHealth = 5;
    string prefabName = "DefaultEnemy";
    string id;

    public virtual string getPrefabName(){
        return prefabName;
    }

    public void buildEnemy(GameObject prefab, Vector2 location)
    {
        DefaultEnemyFactory enemyFactory = new DefaultEnemyFactory();
        enemyFactory.generateEnemy(this, prefab, location);
    }

    public int getHealth()
    {
        return health;
    }

    public int getMaxHealth()
    {
        return maxHealth;
    }

    public int changeHealth(int x)
    {
        health = health + x;
        return health;
    }

    public void setId(string id){
        this.id = id;
    }

    public string getId(){
        return id;
    }
}
