using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityAbilityTriggeredVFX : EffectStep {

    [Tooltip("Shown in the trigger badge over the unit. Leave empty for the unit's own passive (its name); relics set their title.")]
    [SerializeField]
    private string label = "";
    [Tooltip("Icon in the trigger badge (a relic's icon, which also pulses in the unit's status row).")]
    [SerializeField]
    private Sprite icon = null;

    public EntityAbilityTriggeredVFX() {
        effectStepName = "EntityAbilityTriggeredVFX";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CompanionInstance> companions = document.map.TryGetList<CompanionInstance>(EffectDocument.ORIGIN);
        if (companions.Count == 1) {
            CoroutineRunner.Instance.Run(companions[0].companionView.AbilityActivatedVFX());
            CoroutineRunner.Instance.Run(companions[0].companionView.AbilityTriggeredBadge(label, icon));
            yield break;
        }

        List<EnemyInstance> enemies = document.map.TryGetList<EnemyInstance>(EffectDocument.ORIGIN);
        if (enemies.Count == 1) {
            CoroutineRunner.Instance.Run(enemies[0].enemyView.AbilityActivatedVFX());
            CoroutineRunner.Instance.Run(enemies[0].enemyView.AbilityTriggeredBadge(label, icon));
            yield break;
        }
    }
}