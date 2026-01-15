using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
At the end of this, we'll want to be saving and loading one "gameSave" object, which will contain all the data we want to save.

*/
public class SaveManager : GenericSingleton<SaveManager>
{
    public GameStateVariableSO gameStateVariableSO;
    public SORegistry soRegistry;

    // Start is called before the first frame update
    void Start() { }

    public void SaveHandler()
    {
        SaveState saveState = new SaveState("save game", gameStateVariableSO);
        SaveSystem.Save<SaveState>(saveState);
        SavePlayerProgress();
    }

    public void LoadHandler()
    {
        Debug.Log("HELLO THE LOAD HANDLER BUTTON WAS PRESSED FOR SURE");
        SaveState loadState = SaveSystem.Load<SaveState>();
        loadState.Print();
        // instantiate the save state from the serializable data
        loadState.LoadToGameState(gameStateVariableSO, soRegistry);
        // print out a buncha companion information
        foreach (Companion companion in gameStateVariableSO.companions.activeCompanions)
        {
            Debug.Log("CompanionType: " + companion.companionType.name + " loaded");
        }

        foreach (Encounter encounter in gameStateVariableSO.map.GetValue().encounters)
        {
            Debug.Log("EncounterType: " + encounter.getEncounterType() + " loaded");
        }

        gameStateVariableSO.LoadNextLocationFromLoadingSave();
    }

    public bool DoesSaveGameExist()
    {
        return SaveSystem.SaveExists();
    }

    [ContextMenu("Delete Save Data")]
    public void DeleteSaveData()
    {
        SaveSystem.DeleteSave();
    }

    [ContextMenu("Delete Player Progress Data")]
    public void DeletePlayerProgressData()
    {
        SaveSystem.DeleteSave(SaveSystem.SaveType.Progress);
        ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked = -1;
        foreach (AchievementSO achievementSO in ProgressManager.Instance.achievementSOList)
        {
            achievementSO.isCompleted = false;
            achievementSO.currentProgress = 0;
            achievementSO.lockedInProgress = 0;
        }
        gameStateVariableSO.HasSeenCombatTutorial = false;
        gameStateVariableSO.HasSeenPackSelectTutorial = false;
        gameStateVariableSO.HasSeenShopTutorial = false;
        gameStateVariableSO.unlockedCards = new List<CardType>();
    }

    [ContextMenu("Unlock all Achievements and Ascensions")]
    public void UnlockAllAchievementsAndAscensions()
    {
        ProgressManager.Instance.ascensionInfo.playersMaxAscensionUnlocked = ProgressManager.Instance.ascensionInfo.ascensionSOList.Count - 1;
        foreach (AchievementSO achievementSO in ProgressManager.Instance.achievementSOList)
        {
            achievementSO.isCompleted = true;
            achievementSO.currentProgress = achievementSO.target;
            achievementSO.lockedInProgress = achievementSO.target;
        }
        SavePlayerProgress();
    }

    [ContextMenu("Print the Save State")]
    public void PrintSaveState() {
        SaveState loadState = SaveSystem.Load<SaveState>();
        loadState.Print();
    }

    public void GameStartHandler()
    {
        LoadPlayerProgress();
        PlayerSettingsState playerSettingsState = SaveSystem.Load<PlayerSettingsState>(SaveSystem.SaveType.Settings);
        Debug.Log("PlayerSettingsState loaded: " + (playerSettingsState != null));
        playerSettingsState?.LoadPlayerSettings();

        EntityVictoryStatsState entityVictoryStatsState = SaveSystem.Load<EntityVictoryStatsState>(SaveSystem.SaveType.EntityVictoryStats);
        entityVictoryStatsState?.LoadToLocalEntityVictoryState();
    }

    public void LoadPlayerProgress()
    {
        PlayerProgressState playerProgressState = SaveSystem.Load<PlayerProgressState>(SaveSystem.SaveType.Progress);
        playerProgressState?.LoadToLocalPlayerProgress(gameStateVariableSO, soRegistry);
    }

    public void SavePlayerProgress()
    {
        PlayerProgressState playerProgressState = new(ProgressManager.Instance.achievementSOList, gameStateVariableSO);
        SaveSystem.Save<PlayerProgressState>(playerProgressState, SaveSystem.SaveType.Progress);
        playerProgressState.PrintPlayerProgress();
    }

    public void SavePlayerSettings()
    {
        PlayerSettingsState playerSettingsState = new PlayerSettingsState();
        SaveSystem.Save<PlayerSettingsState>(playerSettingsState, SaveSystem.SaveType.Settings);
    }

    public void SaveEntityVictoryStats()
    {
        EntityVictoryStatsState entityVictoryStatsState = new(EntityVictoryStatsManager.Instance.entityVictoryStatsDictionary);
        SaveSystem.Save<EntityVictoryStatsState>(entityVictoryStatsState, SaveSystem.SaveType.EntityVictoryStats);
    }
}