using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerDataVariable",
    menuName = "Player/Player Data Variable")]
public class PlayerDataVariableSO : VariableSO<PlayerData> {
    public void initialize() {
        SetValue(new PlayerData());
        GetValue().gold = 3;
        GetValue().seenTutorial = false;
        GetValue().inTutorialRun = true;
    }

    public void respawn() {
        SetValue(new PlayerData());
        GetValue().gold = 3;
        GetValue().seenTutorial = true;
        GetValue().inTutorialRun = false;
    }
}

