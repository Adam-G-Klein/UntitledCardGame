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
    WAKE_UP_ROOM,
    TEAM_SIGNING,
    MAP,
    TEAM_SELECT,
    PRE_COMBAT_SPLASH,
    COMBAT,
    POST_COMBAT,
    SHOP,
    FAKE_SHOP,
    PRE_BOSSFIGHT_COMBAT_SPLASH,
    BOSSFIGHT,
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
    public List<PackSO> previouslySelectedPackSOs;
    public int ascensionLevel = -1;
    public Dictionary<Location, string> locationToScene = new Dictionary<Location, string>() {
        {Location.MAIN_MENU, "MainMenu"},
        {Location.TEAM_SIGNING, "TeamSigning"},
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

    private Dictionary<Location, Location> locationToNextLocation = new Dictionary<Location, Location>() {
        {Location.MAIN_MENU, Location.INTRO_CUTSCENE},
        {Location.INTRO_CUTSCENE, Location.TUTORIAL},
        {Location.TUTORIAL, Location.TEAM_SIGNING},
        {Location.SHOP_TUTORIAL, Location.SHOP},
        {Location.TEAM_SIGNING, Location.COMBAT},
        {Location.COMBAT, Location.POST_COMBAT},
        {Location.POST_COMBAT, Location.SHOP},
        {Location.SHOP, Location.COMBAT},
        {Location.PACK_SELECT, Location.TEAM_SIGNING},
        {Location.PACK_SELECT_TUTORIAL, Location.PACK_SELECT}
    };


    [Header("Settings for the demo")]
    public int lastTutorialLoopIndex = 2;
    public int bossFightLoopIndex = 6;
    public int tutorialLoops = 2;
    public bool fullscreenEnabled = true;

    // TODO: make this more versatile if we want the map to actually do things.
    // also want to field criticism about whether this should live here.
    // My only argument for placing it here is that this is one of the main
    // functions of the GameStateVariable, and the logic is based entirely on
    // what our current game state is. Lmk your thoughts though.
    public void LoadNextLocation(Location nextLocation = Location.NONE) {
        Debug.Log("Loading next location, current location is: " + currentLocation);
        if(nextLocation == Location.NONE && !locationToNextLocation.ContainsKey(currentLocation)) {
            Debug.LogError("Location " + currentLocation + " is not in the locationToNextLocation dictionary");
            return;
        }
        // Thought about breaking this up and just special-casing the main menu, combat, shop, tutorial,
        // and map, but at that point I think the switch case is nicer.
        // We also have a nice place for other between scene logic additions to live
        switch(currentLocation) {
            case Location.MAIN_MENU:
                nextMapIndex = 0;
                nextEncounter.SetValue(map.GetValue().encounters[nextMapIndex]);
                AdvanceEncounter();
                // The following is only necessary while we don't want the pack selection tutorial early in the experience as it currently looks bad
                if (skipTutorials) {
                    currentLocation = Location.PACK_SELECT;
                }
                if (!hasSeenCombatTutorial) {
                    currentLocation = locationToNextLocation[currentLocation];
                    break;
                }
                if (hasSeenCombatTutorial && hasSeenShopTutorial)
                {
                    if (hasSeenPackSelectTutorial)
                    {
                        currentLocation = Location.PACK_SELECT;
                    }
                    else
                    {
                        currentLocation = Location.PACK_SELECT_TUTORIAL;
                    }
                }
                else {
                    currentLocation = Location.TEAM_SIGNING;
                }
                break;
            case Location.PACK_SELECT_TUTORIAL:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.INTRO_CUTSCENE:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.TEAM_SIGNING:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.TUTORIAL:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.TEAM_SELECT:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.COMBAT:
                Debug.Log("Leaving combat, current location is: " + currentLocation);
                currentEncounterIndex++;
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.POST_COMBAT:
                Debug.Log("Leaving post combat, current location is: " + currentLocation);
                if (skipTutorials || hasSeenShopTutorial) {
                    currentLocation = locationToNextLocation[currentLocation];
                    AdvanceEncounter();
                } else {
                    currentLocation = Location.SHOP_TUTORIAL;
                }
                break;
            case Location.SHOP_TUTORIAL:
                SaveManager.Instance.SaveHandler();
                currentLocation = locationToNextLocation[currentLocation];
                AdvanceEncounter();
                break;
            case Location.SHOP:
                currentEncounterIndex++;
                currentLocation = locationToNextLocation[currentLocation];
                EndTutorialLoop();
                AdvanceEncounter();
                break;
            case Location.PACK_SELECT:
                currentLocation = locationToNextLocation[currentLocation];
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
            if (preferredActiveSlotIndex >= 0) {
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
        setMapGenerator(mapGeneratorSO);
        map.SetValue(mapGenerator.generateMap());
        playerData.initialize(baseShopData);
        viewedSequences = new List<DialogueSequenceSO>();
        SetLocation(Location.MAIN_MENU);
        LoadNextLocation();
    }
}
