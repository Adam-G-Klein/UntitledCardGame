using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ChangeEnemyIntent : EffectStep {

    [SerializeField]
    private string enemyInputKey = "";

    public ChangeEnemyIntent() {
        effectStepName = "ChangeEnemyIntent";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<EnemyInstance> enemies = document.map.TryGetList<EnemyInstance>(enemyInputKey);
        if (enemies.Count != 1) {
            EffectError("ChangeEnemyIntent should only be used with one enemy instance in the key: " + enemyInputKey);
            yield break;
        }

        EnemyInstance enemy = enemies[0];
        enemy.RetargetIntent();
    }
}
