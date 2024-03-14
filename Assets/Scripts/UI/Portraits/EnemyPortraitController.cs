using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPortraitController : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private GameObject portraitParentGO;

    public void Awake() {
    }
    
    public void SetupEnemyPortraits(List<EnemyInstance> enemies) {
        foreach (EnemyInstance enemy in enemies) {
            GameObject parentPortrait = Instantiate(
                portraitParentGO,
                Vector3.zero,
                Quaternion.identity,
                parentTransform);
            EnemyPortrait portrait = parentPortrait.GetComponent<EnemyPortrait>();
            portrait.Setup(enemy);
        }
    }
}
