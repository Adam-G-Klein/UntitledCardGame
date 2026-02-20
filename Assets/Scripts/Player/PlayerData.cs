using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int gold = 0;
    public int shopLevel = 0;
    public int shopLevelIncrementsEarned = 0;
    public int manaPerTurn = 3;
    public int teamSize = 4;
    // TODO: make this a scriptableobject for things they have and haven't done
    public int storedRerolls = 0;
    public int storedCardRemovals = 0;
    public bool seenTutorial = false;
    public bool inTutorialRun = true;

    public PlayerData() {}

    public PlayerData(PlayerDataSerializable playerDataSerializeable) {
        this.gold = playerDataSerializeable.gold;
        this.shopLevel = playerDataSerializeable.shopLevel;
        this.shopLevelIncrementsEarned = playerDataSerializeable.shopLevelIncrementsEarned;
        this.manaPerTurn = playerDataSerializeable.manaPerTurn;
        this.teamSize = playerDataSerializeable.teamSize;
        this.storedRerolls = playerDataSerializeable.storedRerolls;
        this.storedCardRemovals = playerDataSerializeable.storedCardRemovals;
        this.seenTutorial = playerDataSerializeable.seenTutorial;
    }
}

[System.Serializable]
public class PlayerDataSerializable {
    public int gold;
    public int shopLevel;
    public int shopLevelIncrementsEarned;
    public int manaPerTurn;
    public int teamSize;
    public int storedRerolls;
    public int storedCardRemovals;
    public bool seenTutorial;
    public bool inTutorialRun;

    public PlayerDataSerializable(PlayerData playerData) {
        this.gold = playerData.gold;
        this.shopLevel = playerData.shopLevel;
        this.shopLevelIncrementsEarned = playerData.shopLevelIncrementsEarned;
        this.manaPerTurn = playerData.manaPerTurn;
        this.teamSize = playerData.teamSize;
        this.storedRerolls = playerData.storedRerolls;
        this.storedCardRemovals = playerData.storedCardRemovals;
        this.seenTutorial = playerData.seenTutorial;
        this.inTutorialRun = playerData.inTutorialRun;
    }
}