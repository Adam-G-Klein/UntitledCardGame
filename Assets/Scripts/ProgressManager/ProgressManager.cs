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
    public PersistentProgressSO progressSO;
    public CardUnlockerSO cardUnlockerSO;

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
        // if the ascensipn is not present in the list, it is disabled.
        int index = ascensionInfo.ascensionSOList.FindIndex(x => x.ascensionType == ascensionType);
        return index >= 0 && index <= gameState.ascensionLevel;
    }

    [ContextMenu("Unlock Cards")]
    public List<CardType> UnlockCards() {
        List<CardType> cardsToUnlock = cardUnlockerSO.ChooseCardsToUnlock(gameState, progressSO.unlockedCards);
        progressSO.unlockedCards.AddRange(cardsToUnlock);
        progressSO.cardsUnlockedThisRun = new List<CardType>(cardsToUnlock);
        return cardsToUnlock;
    }
}
