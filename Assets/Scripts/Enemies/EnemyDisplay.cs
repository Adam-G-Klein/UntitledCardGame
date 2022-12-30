using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyInstance))]
public class EnemyDisplay : MonoBehaviour
{
    // Component will handle all of the code for animating the enemies
    // when we're ready for that

    private EnemyInstance enemyInstance;
    public Image enemyDisplayImage;

    void Start() {
        enemyInstance = GetComponent<EnemyInstance>();
        enemyDisplayImage.sprite = enemyInstance.enemy.enemyType.sprite;

    }
    


}
