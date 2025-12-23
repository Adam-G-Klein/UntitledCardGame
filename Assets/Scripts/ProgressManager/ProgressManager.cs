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

    [ContextMenu("Unlock Random Cards")]
    public void UnlockRandomCards()
    {
        List<PackSO> activePacks = gameState.baseShopData.activePacks;
        List<UnlockableCard> unlockableCards = new List<UnlockableCard>();
        foreach (var pack in activePacks)
        {
            if (pack.unlockableCardPoolSO == null) continue;
            unlockableCards.AddRange(pack.unlockableCardPoolSO.GetAllUnlockableCards());
        }
        // Filter out cards that are already unlocked.
        unlockableCards = unlockableCards.FindAll(x => !x.isUnlocked);
        if (unlockableCards.Count == 0)
        {
            Debug.Log("All cards are already unlocked.");
            return;
        }

        int numToUnlock = Mathf.Max(1, unlockableCards.Count / 4);
        List<UnlockableCard> shuffledCards = new List<UnlockableCard>(unlockableCards);
        // Shuffle the list
        for (int i = 0; i < shuffledCards.Count; i++)
        {
            UnlockableCard temp = shuffledCards[i];
            int randomIndex = UnityEngine.Random.Range(i, shuffledCards.Count);
            shuffledCards[i] = shuffledCards[randomIndex];
            shuffledCards[randomIndex] = temp;
        }

        for (int i = 0; i < numToUnlock; i++)
        {
            Debug.Log($"Unlocking card: {shuffledCards[i].cardType.Name}");
            shuffledCards[i].isUnlocked = true;
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
}
