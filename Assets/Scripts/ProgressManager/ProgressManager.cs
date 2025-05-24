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
    public AchievementDataSO achievementDataSO;
    public void ReportProgressEvent(GameActionType gameActionType, int amount)
    {
        int currentProgress = achievementDataSO.ReportProgressEvent(gameActionType, amount);
        foreach (var achievementSO in achievementSOList)
        {
            if (achievementSO.gameActionType == gameActionType)
            {
                if (currentProgress >= achievementSO.target)
                {
                    achievementSO.isCompleted = true;
                }
                achievementSO.currentProgress = currentProgress;
            }
        }
    }

    public bool IsAchievementCompleted(AchievementSO achievementSO)
    {
        return achievementSO.isCompleted;
    }
    
    public int GetGameActionTypeOccurences(GameActionType gameActionType)
    {
        return achievementDataSO.GetGameActionTypeOccurences(gameActionType);
    }
}
