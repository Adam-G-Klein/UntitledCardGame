using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInScene : MonoBehaviour, EntityInScene
{
    private Enemy enemy;

    public Enemy getEnemy() {
        return enemy;
    }

    public void setEnemy(Enemy enemy) {
        this.enemy = enemy;
    }

    public Entity getEntity() {
        return enemy;
    }
}
