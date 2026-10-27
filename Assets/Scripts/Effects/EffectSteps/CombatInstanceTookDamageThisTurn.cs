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

        int numTimes = EnemyEncounterManager.Instance.combatEncounterState.GetNumTimesCombatInstanceTookDamageThisTurn(target);
        if (numTimesOutputKey != "") {
            document.intMap[numTimesOutputKey] = numTimes;
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
        document.boolMap[outputKey] = numTimes > 0;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}
