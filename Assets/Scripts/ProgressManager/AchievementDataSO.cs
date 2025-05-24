using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(
    fileName = "AchievementDataSO",
    menuName = "ScriptableObjects/AchievementDataSO")]
[System.Serializable]
public class AchievementDataSO : ScriptableObject
{
    [System.Serializable]
    public class GameActionTypeCountObject
    {
        public GameActionType gameActionType;
        public int progress;
    }

    [SerializeField]
    private List<GameActionTypeCountObject> gameActionTypeCountList = new List<GameActionTypeCountObject>();

    public int ReportProgressEvent(GameActionType gameActionType, int amount)
    {
        var entry = gameActionTypeCountList.Find(e => e.gameActionType == gameActionType);
        if (entry == null)
        {
            entry = new GameActionTypeCountObject { gameActionType = gameActionType, progress = 0 };
            gameActionTypeCountList.Add(entry);
        }
        entry.progress += amount;
        return entry.progress;
    }

    public int GetGameActionTypeOccurences(GameActionType gameActionType)
    {
        var entry = gameActionTypeCountList.Find(e => e.gameActionType == gameActionType);
        return entry != null ? entry.progress : 0;
    }

    public void PrintAllAchievementValues()
    {
        foreach (var entry in gameActionTypeCountList)
        {
            Debug.Log($"Achievement Type: {entry.gameActionType}, Progress: {entry.progress}");
        }
    }
}
