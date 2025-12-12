using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*

*/
public class CombatInstanceTookDamageThisTurn : EffectStep
{
    [SerializeField]
    private string inputCombatInstanceKey = "";


    [SerializeField]
    private string outputKey = "";

    public CombatInstanceTookDamageThisTurn() {
        effectStepName = "CombatInstanceTookDamageThisTurn";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> instances = document.map.GetList<CombatInstance>(inputCombatInstanceKey);
        if (instances.Count == 0) {
            EffectError("No input targets present for key " + inputCombatInstanceKey);
            yield break;
        }
        if (instances.Count > 1) {
            EffectError("Too many input targets for key " + inputCombatInstanceKey + ", only 1 allowed instead have " + instances.Count.ToString());
            yield break;
        }
        CombatInstance target = instances[0];

        bool tookDamage = EnemyEncounterManager.Instance.combatEncounterState.DidCombatInstanceTakeDamageThisTurn(target);
        document.boolMap[outputKey] = tookDamage;
        yield return null;
    }
}
