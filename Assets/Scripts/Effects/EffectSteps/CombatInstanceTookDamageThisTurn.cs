using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*

*/
public class CombatInstanceTookDamageThisTurn : EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private string inputCombatInstanceKey = "";


    [SerializeField]
    private string outputKey = "";

    [SerializeField]
    private string numTimesOutputKey = "";
    [SerializeField]
    private string numRatsWhoLostHPOutputKey = "";

    public CombatInstanceTookDamageThisTurn() {
        effectStepName = "CombatInstanceTookDamageThisTurn";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> instances = document.map.GetList<CombatInstance>(inputCombatInstanceKey);
        if (instances.Count == 0) {
            EffectError("No input targets present for key " + inputCombatInstanceKey);
            yield break;
        }
        CombatInstance target = instances[0];

        if (numTimesOutputKey != "") {
            int count = 0;
            foreach (CombatInstance instance in instances)
            {
                count += EnemyEncounterManager.Instance.combatEncounterState.GetNumTimesCombatInstanceTookDamageThisTurn(instance);
            }
            document.intMap[numTimesOutputKey] = count;
        }
        if (numRatsWhoLostHPOutputKey != "")
        {
            int count = 0;
            foreach (CombatInstance instance in instances)
            {
                bool tookdmg = EnemyEncounterManager.Instance.combatEncounterState.GetNumTimesCombatInstanceTookDamageThisTurn(instance) > 0;
                if (tookdmg)
                    count++;
            }
            document.intMap[numRatsWhoLostHPOutputKey] = count;
        }
        document.boolMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.GetNumTimesCombatInstanceTookDamageThisTurn(target) > 0;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}
