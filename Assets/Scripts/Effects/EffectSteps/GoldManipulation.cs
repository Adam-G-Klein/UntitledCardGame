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


        playerData.gold += finalScale;

        // Hack until we deal with not allowing the player to lose money.
        if (playerData.gold < 0) {
            playerData.gold = 0;
        }

        Debug.Log("Giving the player $" + finalScale.ToString() + ", player now has $" + playerData.gold.ToString());

        yield return null;
    }
}
