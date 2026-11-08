using System.Collections.Generic;

// What the end of run progress screen displays, filled in when the run ends
public class RunProgressSummary
{
    public bool wonRun;
    public bool newStakeUnlocked;
    public int combatsWon;
    public int bossesCleared;
    public int combatXP;
    public int bossXP;
    public int ascensionLevel;
    public float ascensionMultiplier;
    public int ascensionBonusXP;
    public int xpGained;
    public int totalXPBefore;
    public int totalXPAfter;
    public int levelBefore;
    public int levelAfter;
    public List<CardType> cardsUnlocked = new();

    public int LevelsGained => levelAfter - levelBefore;
}
