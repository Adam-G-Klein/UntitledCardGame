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
    public AscensionInfo ascensionInfo;
    public GameStateVariableSO gameState;

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
        if (ascensionLevel > ascensionInfo.playersMaxAscensionUnlocked)
        {
            ascensionInfo.playersMaxAscensionUnlocked = ascensionLevel;
        }
    }

    public AscensionSO GetAscensionSO(AscensionType ascensionType)
    {
        return ascensionInfo.ascensionSOList.Find(x => x.ascensionType == ascensionType);
    }

    public bool IsFeatureEnabled(AscensionType ascensionType)
    {
        // check the index of the ascensionType in the ascensionSOList against the current ascension level from gamestate
        return ascensionInfo.ascensionSOList.FindIndex(x => x.ascensionType == ascensionType) <= gameState.ascensionLevel;
    }
}
