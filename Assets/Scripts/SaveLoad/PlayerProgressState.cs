using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
     [OptionalField]
     private int ascensionIndexVersion;
     [OptionalField]
     private int totalXP;
     private List<string> unlockedCardGuids;
     private bool skipTutorials;
     private bool hasSeenCombatTutorial;
     private bool hasSeenShopTutorial;
     private bool hasCompletedTutorialRun;
     private string nextTutorialID;

     public PlayerProgressState(List<AchievementSO> achievementSOList, GameStateVariableSO gameState, List<CardType> unlockedCards)
     {
          this.numAttackCardsPlayed = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.ZERO_COST_ATTACKS_PLAYED));
          this.hasWonRun = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.WIN_A_RUN));
          this.maxAscensionUnlocked = ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked;
          this.ascensionIndexVersion = AscensionInfo.AscensionIndexVersion;
          this.totalXP = ProgressManager.Instance.progressSO.totalXP;

          // Not yet implemented
          //this.numCardsDiscarded = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.DISCARD_A_CARD));
          //this.healthGained = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.HEALTH_GAINED));
          //this.statusCardsGained = new AchievementSerializable(achievementSOList.Find(x => x.gameActionType == GameActionType.STATUS_CARDS_GAINED));

          this.unlockedCardGuids = unlockedCards.Select(card => card.GUID).ToList();

          this.skipTutorials = gameState.skipTutorials;
          this.hasSeenCombatTutorial = gameState.HasSeenCombatTutorial;
          this.hasSeenShopTutorial = gameState.HasSeenAllShopTutorials;
          this.hasCompletedTutorialRun = gameState.HasCompletedTutorialRun;
          // TutorialManager only exists once the main menu or a combat has loaded, e.g. not when playing a scene directly
          this.nextTutorialID = TutorialManager.Instance != null
               ? TutorialManager.Instance.UpcomingTutorialID
               : SaveSystem.Load<PlayerProgressState>(SaveSystem.SaveType.Progress)?.nextTutorialID;

     }

     public void LoadToLocalPlayerProgress(GameStateVariableSO gameState, SORegistry soRegistry)
     {
          CopyAchievementProgress(this.numAttackCardsPlayed, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.ZERO_COST_ATTACKS_PLAYED));
          CopyAchievementProgress(this.hasWonRun, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.WIN_A_RUN));

          // Not yet implemented
          // CopyAchievementProgress(this.numCardsDiscarded, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.DISCARD_A_CARD));
          // CopyAchievementProgress(this.healthGained, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.HEALTH_GAINED));
          // CopyAchievementProgress(this.statusCardsGained, ProgressManager.Instance.achievementSOList.Find(x => x.gameActionType == GameActionType.STATUS_CARDS_GAINED));

          ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked =
               this.ascensionIndexVersion < AscensionInfo.AscensionIndexVersion ? this.maxAscensionUnlocked + 1 : this.maxAscensionUnlocked;

          ProgressManager.Instance.progressSO.totalXP = this.totalXP;

          ProgressManager.Instance.progressSO.unlockedCards = new List<CardType>();
          // Backwards compatability, not actually sure if this if is necessary
          if (unlockedCardGuids != null) {
               foreach (string guid in unlockedCardGuids) {
                    CardType cardType = soRegistry.GetAsset<CardType>(guid);
                    if (cardType == null) {
                         Debug.LogError($"LoadToLocalPlayerProgress: No card type found in registry for GUID {guid}!");
                         continue;
                    }
                    ProgressManager.Instance.progressSO.unlockedCards.Add(cardType);
               }
          } else {
               Debug.LogError("PlayerProgressState test");
          }

          gameState.skipTutorials = this.skipTutorials;
          gameState.HasSeenCombatTutorial = this.hasSeenCombatTutorial;
          gameState.HasSeenAllShopTutorials = this.hasSeenShopTutorial;
          gameState.HasCompletedTutorialRun = this.hasCompletedTutorialRun;
          if (TutorialManager.Instance != null) TutorialManager.Instance.SetUpcomingTutorialID(this.nextTutorialID);

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
          Debug.Log($"ProgressManager - Total XP: {ProgressManager.Instance.progressSO.totalXP}");
     }

     private void PrintAchievementProgress(AchievementSO achievementSO)
     {
          Debug.Log($"ProgressManager - Achievement: {achievementSO.achievementName}, Current Progress: {achievementSO.currentProgress}, Locked In Progress: {achievementSO.lockedInProgress}, Is Completed: {achievementSO.isCompleted}");
     }
}