using System.Collections;
using UnityEngine;

public class GoldManipulation : EffectStep
{
    [SerializeField]
    public int goldToAdd = 0;

    public override IEnumerator invoke(EffectDocument document) {
        PlayerData playerData = EnemyEncounterManager.Instance.gameState.playerData.GetValue();

        playerData.gold += goldToAdd;

        yield return null;
    }
}
