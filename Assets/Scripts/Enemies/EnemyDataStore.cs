using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDataStore : MonoBehaviour
{
    private Enemy enemy;

    public Enemy getEnemy() {
        return enemy;
    }

    public void setEnemy(Enemy enemy) {
        this.enemy = enemy;
    }
}
