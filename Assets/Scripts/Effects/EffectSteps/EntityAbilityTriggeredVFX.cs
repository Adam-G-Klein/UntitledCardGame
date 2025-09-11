using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityAbilityTriggeredVFX : EffectStep {

    public EntityAbilityTriggeredVFX() {
        effectStepName = "EntityAbilityTriggeredVFX";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CompanionInstance> companions = document.map.TryGetList<CompanionInstance>(EffectDocument.ORIGIN);
        if (companions.Count == 1) {
            yield return companions[0].companionView.AbilityActivatedVFX();
            yield break;
        }
        
        List<EnemyInstance> enemies = document.map.TryGetList<EnemyInstance>(EffectDocument.ORIGIN);
        if (enemies.Count == 1) {
            yield return enemies[0].enemyView.AbilityActivatedVFX();
            yield break;
        }
    }
}