using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Location
{
    NONE,
    MAIN_MENU,
    WAKE_UP_ROOM, // Legacy
    STARTING_TEAM,
    MAP, // Legacy
    TEAM_SELECT, // Legacy
    PRE_COMBAT_SPLASH, // Legacy
    COMBAT,
    POST_COMBAT,
    SHOP,
    FAKE_SHOP, // Legacy
    PRE_BOSSFIGHT_COMBAT_SPLASH, // Legacy
    BOSSFIGHT, // Legacy
    INTRO_CUTSCENE,
    TUTORIAL,
    SHOP_TUTORIAL,
    PACK_SELECT,
    PACK_SELECT_TUTORIAL,
}

[CreateAssetMenu(
    fileName = "NewGameStateVariable",
    menuName = "Variables/Game State Variable")]
[System.Serializable]
public class GameStateVariableSO : ScriptableObject
{
    public CompanionListVariableSO companions;
    public PlayerDataVariableSO playerData;
    public MapVariableSO map;
    public ShopDataSO baseShopData;
    private MapGeneratorSO mapGenerator;
    [Header("The Combat or Shop Encounter we're currently in")]
    public EncounterVariableSO activeEncounter;
    [Header("Our map and loop index, used to determine things like which dialogue to display")]
    public int nextMapIndex = 0;
    [Header("The Next Combat or Shop Encounter we'll enter")]
    public EncounterVariableSO nextEncounter;
    public Location currentLocation;
    public AllDialogueLocationsSO dialogueLocations;
    [Header("Prevents dialogue seqs from being added to the viewed list")]
    public bool debugSingleEncounterMode = false;
    public List<DialogueSequenceSO> viewedSequences;
    public CompanionInstance hoveredCompanion = null;
    public int currentEncounterIndex = 0;
    public Guid currentRunID;
    [SerializeField]
    public bool skipTutorials = false;
    [SerializeField]
    private bool hasSeenPackSelectTutorial = false;
    public bool HasSeenPackSelectTutorial {
        get => hasSeenPackSelectTutorial;
        set {
            hasSeenPackSelectTutorial = value;
            if (value) {
                SaveManager.Instance.SavePlayerProgress();
            }
        }
    }
    [SerializeField]
    private bool hasSeenShopTutorial = false;
    public bool HasSeenShopTutorial {
        get => hasSeenShopTutorial;
        set {
            hasSeenShopTutorial = value;
            if (value) {
                SaveManager.Instance.SavePlayerProgress();
            }
        }
    }
    [SerializeField]
    private bool hasSeenCombatTutorial = false;
    public bool HasSeenCombatTutorial {
        get => hasSeenCombatTutorial;
        set {
            hasSeenCombatTutorial = value;
            if (value) {
                SaveManager.Instance.SavePlayerProgress();
            }
        }
    }
    public bool autoUpgrade = false;
    public bool demoMode = false;
    // True by default because fuck it we'll get way more data this way.
    // I feel like marky z
    public bool consentToDataCollection = true;

    public List<PackSO> previouslySelectedPackSOs;
    public int ascensionLevel = -1;
    public Dictionary<Location, string> locationToScene = new Dictionary<Location, string>() {
        {Location.MAIN_MENU, "MainMenu"},
        {Location.STARTING_TEAM, "StartingTeam"},
        {Location.TUTORIAL, "TutorialScene"},
        {Location.PACK_SELECT_TUTORIAL, "PackSelectionTutorialScene"},
        {Location.SHOP_TUTORIAL, "ShopTutorialScene"},
        {Location.PACK_SELECT, "PackSelectionScene"},
        {Location.TEAM_SELECT, "TeamSelect"},
        {Location.PRE_COMBAT_SPLASH, "PreCombatSplash"},
        {Location.COMBAT, "CombatScene"},
        {Location.POST_COMBAT, "CombatScene"},
        {Location.SHOP, "PlaceholderShopEncounter"},
        {Location.INTRO_CUTSCENE, "IntroCutscene"}
    };

    [Header("Settings for the demo")]
    public int lastTutorialLoopIndex = 2;
    public int bossFightLoopIndex = 6;
    public int tutorialLoops = 2;
    public bool fullscreenEnabled = true;

    public void LoadNextLocation() {
        Debug.Log("Loading next location, current location is: " + currentLocation);
        switch(currentLocation) {
            case Location.MAIN_MENU:
                nextMapIndex = 0;
                nextEncounter.SetValue(map.GetValue().encounters[nextMapIndex]);
                AdvanceEncounter();

                // The following is only necessary while we don't want the pack selection tutorial early in the experience as it currently looks bad
                if (skipTutorials && !demoMode) {
                    currentLocation = Location.PACK_SELECT;
                    break;
                } else if (skipTutorials) {
                    currentLocation = Location.STARTING_TEAM;
                    break;
                }

                if (!hasSeenCombatTutorial) {
                    currentLocation = Location.INTRO_CUTSCENE;
                    break;
                }

                if (hasSeenCombatTutorial && hasSeenShopTutorial && !demoMode) {
                    if (hasSeenPackSelectTutorial) {
                        currentLocation = Location.PACK_SELECT;
                    }
                    else {
                        currentLocation = Location.PACK_SELECT_TUTORIAL;
                    }
                    break;
                } else {
                    currentLocation = Location.STARTING_TEAM;
                }
                break;
            case Location.PACK_SELECT_TUTORIAL:
                currentLocation = Location.PACK_SELECT;
                break;
            case Location.INTRO_CUTSCENE:
                currentLocation = Location.TUTORIAL;
                break;
            case Location.STARTING_TEAM:
                currentLocation = Location.COMBAT;
                break;
            case Location.TUTORIAL:
                currentLocation = Location.STARTING_TEAM;
                break;
            case Location.COMBAT:
                Debug.Log("Leaving combat, current location is: " + currentLocation);
                currentEncounterIndex++;
                currentLocation = Location.POST_COMBAT;
                break;
            case Location.POST_COMBAT:
                Debug.Log("Leaving post combat, current location is: " + currentLocation);
                if (skipTutorials || hasSeenShopTutorial) {
                    currentLocation = Location.SHOP;
                    AdvanceEncounter();
                } else {
                    currentLocation = Location.SHOP_TUTORIAL;
                }
                break;
            case Location.SHOP_TUTORIAL:
                SaveManager.Instance.SaveHandler();
                currentLocation = Location.SHOP;
                AdvanceEncounter();
                break;
            case Location.SHOP:
                currentEncounterIndex++;
                currentLocation = Location.COMBAT;
                EndTutorialLoop();
                AdvanceEncounter();
                break;
            case Location.PACK_SELECT:
                currentLocation = Location.STARTING_TEAM;
                break;
            default:
                Debug.Log("Invalid location in LoadNextLocation switch case");
                break;
        }

        if (currentLocation == Location.SHOP || currentLocation == Location.COMBAT) {
            SaveManager.Instance.SaveHandler();
        }

        updateMusic(currentLocation);
        cancelCurrentDialogue();
        LoadCurrentLocationScene();
    }

    public void LoadNextLocationFromLoadingSave() {
        // Current encounter index of 0 is the first combat in the game
        Encounter currentEncounter = map.GetValue().encounters[currentEncounterIndex];
        if (currentEncounter.getEncounterType() == EncounterType.Enemy) {
            currentLocation = Location.COMBAT;
        } else if (currentEncounter.getEncounterType() == EncounterType.Shop) {
            currentLocation = Location.SHOP;
        }
        activeEncounter.SetValue(currentEncounter);
        updateMusic(currentLocation);
        cancelCurrentDialogue();
        LoadCurrentLocationScene();
    }

    public void RemoveCompanionsFromTeam(List<Companion> companions) {
        foreach(Companion c in companions) {
            if(this.companions.activeCompanions.Contains(c)) {
                this.companions.activeCompanions.Remove(c);
            }
            if(this.companions.benchedCompanions.Contains(c)) {
                this.companions.benchedCompanions.Remove(c);
            }
        }
    }
    public void AddCompanionToTeam(Companion c, int preferredActiveSlotIndex = -1) {
        if(this.companions.spaceInActiveCompanions) {
            if (preferredActiveSlotIndex >= 0 && preferredActiveSlotIndex <= this.companions.activeCompanions.Count) {
                this.companions.activeCompanions.Insert(preferredActiveSlotIndex, c);
            } else {
                this.companions.activeCompanions.Add(c);
            }
        } else {
            this.companions.benchedCompanions.Add(c);
        }
    }


    public void cancelCurrentDialogue() {
        if(DialogueManager.Instance != null) {
            DialogueManager.Instance.skipCurrentDialogue();
        }
    }

    public void LoadCurrentLocationScene() {
        if(!locationToScene.ContainsKey(currentLocation)) {
            Debug.LogError("ERROR: Invalid location");
            return;
        }
        if(locationToScene[currentLocation] != SceneManager.GetActiveScene().name) {
            SceneTransitionManager.LoadScene(locationToScene[currentLocation]);
        }
    }

    private void EndTutorialLoop() {
        if(GetLoopIndex() >= 2) {
            Debug.Log("Tutorial loop ended");
            playerData.GetValue().seenTutorial = true;
        }
    }

    public void UpgradeTheShop() {
        PlayerData pd = playerData.GetValue();
        pd.shopLevel += 1;
        ShopLevel shopLevel = baseShopData.shopLevels[pd.shopLevel];
        companions.SetCompanionSlots(shopLevel.teamSize);
        pd.shopLevelIncrementsEarned = 0;
        pd.manaPerTurn = shopLevel.mana;
    }

    public bool EarnUpgradeIncrement()
    {
        PlayerData pd = playerData.GetValue();
        ShopLevel shopLevel = baseShopData.shopLevels[pd.shopLevel];
        if (shopLevel.level < baseShopData.shopLevels.Count - 1)
        {
            if (pd.shopLevelIncrementsEarned == shopLevel.shopLevelIncrementsToUnlock - 1)
            {
                UpgradeTheShop();
                return true;
            }
            else
            {
                pd.shopLevelIncrementsEarned += 1;
                return false;
            }
        }
        return false;
    }

    // TODO: Change this logic if we want map choices to ever actually matter
    private void AdvanceEncounter()
    {
        activeEncounter.SetValue(nextEncounter.GetValue());
        Debug.Log("Advanced encounter, new encounter type is : " + activeEncounter.GetValue().getEncounterType());
        nextMapIndex += 1;
        if (nextMapIndex < map.GetValue().encounters.Count)
        {
            nextEncounter.SetValue(map.GetValue().encounters[nextMapIndex]);
        }
        else
        {
            Debug.LogError("ERROR: LoadNextEncounter called, no more encounters in map");
        }
    }

    // Returns the loop we're on, 1 and 2 for the tutorial,
    // used to index into all of the dialogueLocationLists
    // the way to think about it is that we start on loop 0, and each postCombat starts the next loop
    public int GetLoopIndex() {
        Debug.Log("Returning loop index: " + Mathf.FloorToInt(nextMapIndex / 2));
        return Mathf.FloorToInt((nextMapIndex) / 2);
    }

    public void SetLocation(Location newLocation) {
        currentLocation = newLocation;
    }

    public void UpdateHoveredCompanion(CompanionInstance companionInstance) {
        hoveredCompanion = companionInstance;
        PlayerHand.Instance.UpdatePlayableCards();
    }

    public void KillPlayer() {
        Debug.Log("killing player");
        playerData.respawn(baseShopData);
        companions.respawn();
        map.SetValue(mapGenerator.generateMap());
        SetLocation(Location.WAKE_UP_ROOM);
        LoadCurrentLocationScene();

    }

    private void updateMusic(Location newLocation) {
        // Just not changing the music on the map right now
        if(newLocation != Location.MAP && MusicController.Instance != null) {
            MusicController.Instance.PlayMusicLocation(newLocation);
        }
    }

    public void setMapGenerator(MapGeneratorSO mapGeneratorSO) {
        mapGenerator = mapGeneratorSO;
    }

    public void StartNewRun(MapGeneratorSO mapGeneratorSO) {
        currentEncounterIndex = 0;
        currentRunID = Guid.NewGuid();
        setMapGenerator(mapGeneratorSO);
        map.SetValue(mapGenerator.generateMap());
        playerData.initialize(baseShopData);
        viewedSequences = new List<DialogueSequenceSO>();

        // Record an event to record the start of the run.
        // We should move this to the pack selection screen or the starting team
        // so we can also record the starting team, configured ascensions, selected packs.
        var startEvent = new RunStartedAnalyticsEvent{};
        AnalyticsManager.Instance.RecordEvent(startEvent);

        SetLocation(Location.MAIN_MENU);
        LoadNextLocation();
    }
}
