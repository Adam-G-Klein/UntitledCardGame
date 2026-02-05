using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class SpriteSwap : EffectStep {

    public Sprite newSprite = null;

    public SpriteSwap() {
        effectStepName = "SpriteSwap";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<EnemyInstance> enemies = document.map.TryGetList<EnemyInstance>(EffectDocument.ORIGIN);
        if (enemies.Count == 1) {
            Debug.Log("SpriteSwap: Swapping sprite for enemy" + enemies[0].enemy.GetName() + " to " + newSprite.name);
            EnemyEncounterManager.Instance.combatEncounterView.UpdateSpriteForEnemy(enemies[0].enemy, newSprite);
            yield break;
        }

    }
}