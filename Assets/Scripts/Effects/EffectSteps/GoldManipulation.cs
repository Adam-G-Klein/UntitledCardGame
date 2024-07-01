using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldManipulation : EffectStep
{
    [SerializeField]
    private int goldToAdd = 0;

    public override IEnumerator invoke(EffectDocument document) {
        PlayerData playerData = EnemyEncounterManager.Instance.gameState.playerData.GetValue();

        
    }
}
