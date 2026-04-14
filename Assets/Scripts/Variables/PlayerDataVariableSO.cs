using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerDataVariable",
    menuName = "Player/Player Data Variable")]
[System.Serializable]
public class PlayerDataVariableSO : VariableSO<PlayerData> {
    public void initialize(int startingGold) {
        SetValue(new PlayerData());
        GetValue().gold = startingGold;
        GetValue().seenTutorial = false;
        GetValue().inTutorialRun = true;
        GetValue().shopLevelIncrementsEarned = 0;
    }

    public void respawn(int startingGold) {
        SetValue(new PlayerData());
        GetValue().gold = startingGold;
        GetValue().seenTutorial = true;
        GetValue().inTutorialRun = false;
        GetValue().shopLevelIncrementsEarned = 0;
    }
}

