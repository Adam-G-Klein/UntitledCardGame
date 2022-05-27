using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultEnemyFactory
{
    public void generateEnemy(DefaultEnemy enemy, GameObject prefab, Vector2 location)
    {
        GameObject enemyGameObject;

        enemyGameObject = Object.Instantiate(
            prefab,
            location, 
            Quaternion.identity) as GameObject;
        enemyGameObject.GetComponent<DisplayHealth>().setEntity(enemy);
        enemyGameObject.GetComponent<EnemyDataStore>().setEnemy(enemy);
    }
}
