using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class PlayerProgressState
{
     private AchievementSerializable hasWonRun;
     private AchievementSerializable numAttackCardsPlayed;
     private AchievementSerializable numCardsDiscarded;
     private AchievementSerializable healthGained;
     private AchievementSerializable statusCardsGained;
     private int maxAscensionUnlocked;
     private bool skipTutorials;
     private bool hasSeenCombatTutorial;
     private bool hasSeenShopTutorial;
     private bool hasSeenPackSelectTutorial;
     private string nextTutorialID;

     public PlayerProgressState(List<AchievementSO> achievementSOList, GameStateVariableSO gameState)
     {
          this.numAttackCardsPlayed = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.ZERO_COST_ATTACKS_PLAYED));
          this.hasWonRun = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.WIN_A_RUN));
          this.maxAscensionUnlocked = ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked;

          // Not yet implemented 
          //this.numCardsDiscarded = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.DISCARD_A_CARD));
          //this.healthGained = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.HEALTH_GAINED));
          //this.statusCardsGained = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.STATUS_CARDS_GAINED));

          this.skipTutorials = gameState.skipTutorials;
          this.hasSeenCombatTutorial = gameState.HasSeenCombatTutorial;
          this.hasSeenPackSelectTutorial = gameState.HasSeenPackSelectTutorial;
          this.hasSeenShopTutorial = gameState.HasSeenShopTutorial;
          this.nextTutorialID = TutorialManager.Instance.UpcomingTutorialID;
     }

     public void LoadToLocalPlayerProgress(GameStateVariableSO gameState)
     {
          CopyAchievementProgress(this.numAttackCardsPlayed, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.ZERO_COST_ATTACKS_PLAYED));
          CopyAchievementProgress(this.hasWonRun, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.WIN_A_RUN));

          // Not yet implemented
          // CopyAchievementProgress(this.numCardsDiscarded, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.DISCARD_A_CARD));
          // CopyAchievementProgress(this.healthGained, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.HEALTH_GAINED));
          // CopyAchievementProgress(this.statusCardsGained, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.STATUS_CARDS_GAINED));

          ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked = this.maxAscensionUnlocked;

          gameState.skipTutorials = this.skipTutorials;
          gameState.HasSeenCombatTutorial = this.hasSeenCombatTutorial;
          gameState.HasSeenPackSelectTutorial = this.hasSeenPackSelectTutorial;
          gameState.HasSeenShopTutorial = this.hasSeenShopTutorial;
          TutorialManager.Instance.SetUpcomingTutorialID(this.nextTutorialID);

          PrintPlayerProgress();
     }

     private void CopyAchievementProgress(AchievementSerializable from, AchievementSO to)
     {
          to.currentProgress = from.currentProgress;
          to.lockedInProgress = from.lockedInProgress;
          to.isCompleted = from.isCompleted;
     }

     public void PrintPlayerProgress()
     {
          PrintAchievementProgress(ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.ZERO_COST_ATTACKS_PLAYED));
          PrintAchievementProgress(ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.WIN_A_RUN));

          // Not yet implemented
          // PrintAchievementProgress(ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.DISCARD_A_CARD));
          // PrintAchievementProgress(ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.HEALTH_GAINED));
          // PrintAchievementProgress(ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.STATUS_CARDS_GAINED));
          Debug.Log($"ProgressManager - Max Ascension Unlocked: {ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked}");
     }
     
     private void PrintAchievementProgress(AchievementSO achievementSO)
     {
          Debug.Log($"ProgressManager - Achievement: {achievementSO.achievementName}, Current Progress: {achievementSO.currentProgress}, Locked In Progress: {achievementSO.lockedInProgress}, Is Completed: {achievementSO.isCompleted}");
     }
}