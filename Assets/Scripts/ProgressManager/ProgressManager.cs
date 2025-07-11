using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public enum GameActionType
{
    WIN_A_RUN,
    DISCARD_A_CARD,
    HEALTH_GAINED,
    STATUS_CARDS_GAINED,
    ZERO_COST_ATTACKS_PLAYED,
}

[System.Serializable]
public class ProgressManager : GenericSingleton<ProgressManager>
{
    public List<AchievementSO> achievementSOList;
    public int playersMaxAscensionUnlocked = 0;
    public void ReportProgressEvent(GameActionType gameActionType, int amount)
    {
        AchievementSO achievementSO = achievementSOList.Find(x => x.gameActionType == gameActionType);
        achievementSO.currentProgress += amount;
        if (achievementSO.currentProgress >= achievementSO.target)
        {
            achievementSO.isCompleted = true;
        }
    }

    public bool IsAchievementCompleted(AchievementSO achievementSO)
    {
        return achievementSO.isCompleted;
    }

    public void SetMaxAscensionUnlocked(int ascensionLevel)
    {
        if (ascensionLevel > playersMaxAscensionUnlocked)
        {
            playersMaxAscensionUnlocked = ascensionLevel;
        }
    }
}
