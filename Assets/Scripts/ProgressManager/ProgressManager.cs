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
    [Tooltip("Every released pack. Used to retroactively unlock cards on load, when there is no run team/active packs to draw from.")]
    public List<PackSO> allUnlockablePacks;

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

    // Retroactively unlocks cards the player is owed for their current level, e.g. XP earned before
    // card unlocks existed. Max level players also get every card that's still locked, so cards added
    // in later updates reach them. Doesn't touch the end of run summary, so this is silent.
    public List<CardType> CatchUpCardUnlocks() {
        List<CardType> cardsToUnlock = new List<CardType>();
        if (gameState.BuildTypeDemoOrConvention()) return cardsToUnlock;
        if (playerLevelingSO == null) {
            Debug.LogError("ProgressManager has no PlayerLevelingSO assigned, can't catch up card unlocks");
            return cardsToUnlock;
        }

        int level = playerLevelingSO.GetLevel(progressSO.totalXP);
        // With no thresholds MaxLevel is 1, which would count every player as max level.
        bool isMaxLevel = playerLevelingSO.levelXPThresholds.Count > 0 && level >= playerLevelingSO.MaxLevel;

        HashSet<CardType> alreadyUnlocked = new HashSet<CardType>(progressSO.unlockedCards);
        List<CardType> locked = CardUnlockerSO.GetUnlockableCards(allUnlockablePacks)
            .FindAll(card => !alreadyUnlocked.Contains(card));

        int owed = (level - 1) * playerLevelingSO.cardsUnlockedPerLevel;
        int count = isMaxLevel ? locked.Count : Mathf.Max(0, owed - progressSO.unlockedCards.Count);
        count = Mathf.Min(count, locked.Count);

        for (int i = 0; i < count; i++) {
            int randomIndex = UnityEngine.Random.Range(i, locked.Count);
            (locked[i], locked[randomIndex]) = (locked[randomIndex], locked[i]);
            Debug.Log($"Catch-up unlocking card: {locked[i].GetName()}");
            cardsToUnlock.Add(locked[i]);
        }
        progressSO.unlockedCards.AddRange(cardsToUnlock);
        Debug.Log($"Card unlock catch-up: level {level}{(isMaxLevel ? " (max)" : "")}, owed {owed}, had {alreadyUnlocked.Count}, {locked.Count} locked, unlocked {cardsToUnlock.Count}");
        return cardsToUnlock;
    }

    [ContextMenu("Run Card Unlock Catch-Up")]
    private void RunCardUnlockCatchUp() {
        if (CatchUpCardUnlocks().Count > 0) {
            SaveManager.Instance.SavePlayerProgress();
        }
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
