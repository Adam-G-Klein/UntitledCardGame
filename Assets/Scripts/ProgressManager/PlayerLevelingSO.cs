using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerLevelingSO",
    menuName = "ScriptableObjects/Player Leveling SO")]
public class PlayerLevelingSO : ScriptableObject
{
    [Header("XP earned per run")]
    public int xpPerCombatWon = 10;
    [Tooltip("Awarded on top of xpPerCombatWon for elite (act-ending) fights and the final boss")]
    public int xpPerBossCleared = 25;
    [Tooltip("Added to the XP multiplier per ascension stake, e.g. 0.25 makes stake 2 award 1.5x")]
    public float bonusMultiplierPerAscension = 0.25f;

    [Header("Rewards")]
    public int cardsUnlockedPerLevel = 3;

    [Header("Level checkpoints")]
    [Tooltip("Ascending total XP needed to reach level 2, 3, 4... Players start at level 1")]
    public List<int> levelXPThresholds = new();

    public int MaxLevel => levelXPThresholds.Count + 1;

    public float GetAscensionMultiplier(int ascensionLevel)
    {
        return 1f + bonusMultiplierPerAscension * Mathf.Max(0, ascensionLevel);
    }

    public int CalculateRunXP(int combatsWon, int bossesCleared, int ascensionLevel)
    {
        return CalculateBaseXP(combatsWon, bossesCleared) + CalculateAscensionBonusXP(combatsWon, bossesCleared, ascensionLevel);
    }

    public int CalculateBaseXP(int combatsWon, int bossesCleared)
    {
        return combatsWon * xpPerCombatWon + bossesCleared * xpPerBossCleared;
    }

    public int CalculateAscensionBonusXP(int combatsWon, int bossesCleared, int ascensionLevel)
    {
        int baseXP = CalculateBaseXP(combatsWon, bossesCleared);
        return Mathf.RoundToInt(baseXP * GetAscensionMultiplier(ascensionLevel)) - baseXP;
    }

    public int GetLevel(int totalXP)
    {
        int level = 1;
        foreach (int threshold in levelXPThresholds)
        {
            if (totalXP < threshold) break;
            level++;
        }
        return level;
    }

    // Total XP at which the given level begins
    public int GetLevelStartXP(int level)
    {
        if (level <= 1) return 0;
        return levelXPThresholds[Mathf.Min(level, MaxLevel) - 2];
    }

    // Total XP needed to reach the level after the given one, or -1 at max level
    public int GetNextLevelXP(int level)
    {
        if (level >= MaxLevel) return -1;
        return levelXPThresholds[level - 1];
    }
}
