using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The effect that taunts an enemy, making it attack the friendly entity that delt
    the card.

    Input: The enemy target
    Output: NA
    Parameters:

*/
[System.Serializable]
public class Taunt : EffectStep {
    [SerializeField]
    private string inputTargetsKey = "";
    [SerializeField]
    private string inputOriginKey = "";

    public Taunt() {
        effectStepName = "Taunt";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<EnemyInstance> enemies = document.enemyMap.getList(inputTargetsKey);
        if (enemies.Count == 0) {
            EffectError("No input targets present for key " + inputTargetsKey);
            yield return null;
        }

        List<TargettableEntity> originEntities = document.getTargettableEntities(inputOriginKey);
        if (originEntities.Count == 0 || originEntities.Count > 1) {
            EffectError("None or too many origin entities provided for key " + inputOriginKey);
            yield return null;
        }

        foreach (EnemyInstance enemyInstance in enemies) {
            enemyInstance.setTauntedTarget(originEntities[0]);
        }

        yield return null;
    }
}