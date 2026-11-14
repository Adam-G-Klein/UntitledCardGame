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
    public PlayerLevelingSO playerLevelingSO;

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

    // Returns true if this unlocked a stake the player didn't have yet
    public bool SetMaxAscensionUnlocked(int ascensionLevel)
    {
        if (ascensionLevel >= ascensionInfo.ascensionSOList.Count) return false;
        if (ascensionLevel > ascensionInfo.playersMaxAscensionUnlocked)
        {
            ascensionInfo.playersMaxAscensionUnlocked = ascensionLevel;
            return true;
        }
        return false;
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

    public List<CardType> UnlockCards(int numberToUnlock) {
        List<CardType> cardsToUnlock = numberToUnlock > 0
            ? cardUnlockerSO.ChooseCardsToUnlock(gameState, progressSO.unlockedCards, numberToUnlock)
            : new List<CardType>();
        progressSO.unlockedCards.AddRange(cardsToUnlock);
        progressSO.cardsUnlockedThisRun = new List<CardType>(cardsToUnlock);
        return cardsToUnlock;
    }

    [ContextMenu("Unlock Cards For One Level")]
    private void UnlockCardsForOneLevel() {
        UnlockCards(playerLevelingSO.cardsUnlockedPerLevel);
    }

    // Awards XP for the run, unlocks cards for any levels gained, and records the summary for the end of run screen
    public RunProgressSummary AwardRunXP(bool wonRun, bool newStakeUnlocked)
    {
        progressSO.lastRunSummary = null;
        progressSO.cardsUnlockedThisRun = new List<CardType>();
        if (gameState.BuildTypeDemoOrConvention()) return null;
        if (playerLevelingSO == null)
        {
            Debug.LogError("ProgressManager has no PlayerLevelingSO assigned, no XP awarded");
            return null;
        }

        List<Encounter> encounters = gameState.map.GetValue().encounters;
        int combatsWon = 0;
        int bossesCleared = 0;
        foreach (Encounter encounter in encounters)
        {
            if (!encounter.isCompleted || encounter.getEncounterType() != EncounterType.Enemy) continue;
            combatsWon++;
            if (((EnemyEncounter)encounter).isEliteEncounter) bossesCleared++;
        }
        // The final boss isn't marked completed before the win handler runs
        if (wonRun && !encounters[encounters.Count - 1].isCompleted)
        {
            combatsWon++;
            bossesCleared++;
        }

        int ascensionLevel = gameState.ascensionLevel;
        int ascensionBonusXP = playerLevelingSO.CalculateAscensionBonusXP(combatsWon, bossesCleared, ascensionLevel);
        int xpGained = playerLevelingSO.CalculateRunXP(combatsWon, bossesCleared, ascensionLevel);
        int totalXPBefore = progressSO.totalXP;
        progressSO.totalXP += xpGained;

        RunProgressSummary summary = new RunProgressSummary
        {
            wonRun = wonRun,
            newStakeUnlocked = newStakeUnlocked,
            combatsWon = combatsWon,
            bossesCleared = bossesCleared,
            combatXP = combatsWon * playerLevelingSO.xpPerCombatWon,
            bossXP = bossesCleared * playerLevelingSO.xpPerBossCleared,
            ascensionLevel = ascensionLevel,
            ascensionMultiplier = playerLevelingSO.GetAscensionMultiplier(ascensionLevel),
            ascensionBonusXP = ascensionBonusXP,
            xpGained = xpGained,
            totalXPBefore = totalXPBefore,
            totalXPAfter = progressSO.totalXP,
            levelBefore = playerLevelingSO.GetLevel(totalXPBefore),
            levelAfter = playerLevelingSO.GetLevel(progressSO.totalXP),
        };
        summary.cardsUnlocked = UnlockCards(summary.LevelsGained * playerLevelingSO.cardsUnlockedPerLevel);
        progressSO.lastRunSummary = summary;
        Debug.Log($"Run XP: {combatsWon} combats, {bossesCleared} bosses, x{summary.ascensionMultiplier} = {xpGained} XP. Level {summary.levelBefore} -> {summary.levelAfter}, {summary.cardsUnlocked.Count} cards unlocked");
        return summary;
    }
}
