using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPortraitController : MonoBehaviour
{
    /*
    [SerializeField] private GameObject portraitParentGO;

    public void Awake() {
    }
    
    public void SetupEnemyPortraits(List<EnemyInstance> enemies) {
        Debug.Log("Setting up enemy portraits, count:   " + enemies.Count);
        foreach (EnemyInstance enemy in enemies) {
            GameObject parentPortrait = Instantiate(
                portraitParentGO,
                Vector3.zero,
                Quaternion.identity,
                enemy.transform);
            EnemyPillarController portrait = parentPortrait.GetComponent<EnemyPillarController>();
            portrait.Setup(enemy);
        }
    }
    */
}
