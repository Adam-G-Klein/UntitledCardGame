using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerDataVariable",
    menuName = "Player/Player Data Variable")]
[System.Serializable]
public class PlayerDataVariableSO : VariableSO<PlayerData> {
    public void initialize(ShopDataSO shopData) {
        SetValue(new PlayerData());
        GetValue().gold = shopData.startingGold;
        GetValue().seenTutorial = false;
        GetValue().inTutorialRun = true;
    }

    public void respawn(ShopDataSO shopData) {
        SetValue(new PlayerData());
        GetValue().gold = shopData.startingGold;
        GetValue().seenTutorial = true;
        GetValue().inTutorialRun = false;
    }
}

