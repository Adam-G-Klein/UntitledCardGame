using System.Collections;
using UnityEngine;

public class GoldManipulation : EffectStep
{
    [SerializeField]
    public int goldToAdd = 0;

    [SerializeField]
    public bool getScaleFromKey = false;

    [SerializeField]
    public string inputScaleKey = "";

    public override IEnumerator invoke(EffectDocument document) {
        int finalScale = goldToAdd;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }

        PlayerData playerData = EnemyEncounterManager.Instance.gameState.playerData.GetValue();

        Debug.Log("Giving the player $" + finalScale.ToString());

        playerData.gold += finalScale;

        yield return null;
    }
}
