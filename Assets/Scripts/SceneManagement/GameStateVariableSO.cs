using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Location {
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
    [SerializeField]
    private int currentLoopIndex = 0;
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
    private bool hasSeenTutorial = false;
    public Dictionary<Location, string> locationToScene = new Dictionary<Location, string>() {
        {Location.MAIN_MENU, "MainMenu"},
        {Location.WAKE_UP_ROOM, "AidensRoom"},
        {Location.TEAM_SIGNING, "TeamSigning"},
        {Location.TUTORIAL, "TutorialScene"},
        {Location.SHOP_TUTORIAL, "ShopTutorialScene"},
        {Location.MAP, "Map"},
        {Location.TEAM_SELECT, "TeamSelect"},
        {Location.PRE_COMBAT_SPLASH, "PreCombatSplash"},
        {Location.COMBAT, "CombatScene"},
        {Location.POST_COMBAT, "CombatScene"},
        {Location.SHOP, "PlaceholderShopEncounter"},
        {Location.INTRO_CUTSCENE, "IntroCutscene"},
        // not implemented yet
        {Location.FAKE_SHOP, "FakeShop"},
        {Location.PRE_BOSSFIGHT_COMBAT_SPLASH, "PreBossfightCombatSplash"},
        {Location.BOSSFIGHT, "PlaceholderBossfightEncounter"}
    };

    private Dictionary<Location, Location> locationToNextLocation = new Dictionary<Location, Location>() {
        {Location.MAIN_MENU, Location.INTRO_CUTSCENE},
        /*{Location.INTRO_CUTSCENE, Location.TEAM_SIGNING},
        {Location.TEAM_SIGNING, Location.COMBAT},*/
        {Location.INTRO_CUTSCENE, Location.TUTORIAL},
        {Location.TUTORIAL, Location.TEAM_SIGNING},
        {Location.SHOP_TUTORIAL, Location.SHOP},
        {Location.TEAM_SIGNING, Location.COMBAT},
        {Location.COMBAT, Location.POST_COMBAT},
        {Location.POST_COMBAT, Location.SHOP},
        {Location.SHOP, Location.COMBAT},
        {Location.FAKE_SHOP, Location.PRE_BOSSFIGHT_COMBAT_SPLASH},
        {Location.PRE_BOSSFIGHT_COMBAT_SPLASH, Location.BOSSFIGHT},
        {Location.BOSSFIGHT, Location.MAP}
    };

    private HashSet<Location> resumeableLocations = new HashSet<Location>() {
        {Location.COMBAT},
        {Location.POST_COMBAT},
        {Location.SHOP},
        {Location.FAKE_SHOP},
        {Location.BOSSFIGHT},
        {Location.MAP}
    };


    [Header("Settings for the demo")]
    public int lastTutorialLoopIndex = 2;
    public int bossFightLoopIndex = 6;
    public int tutorialLoops = 2;

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
                if (hasSeenTutorial) {
                    currentLocation = Location.TEAM_SIGNING;
                } else {
                    currentLocation = locationToNextLocation[currentLocation];
                }
/*                GameStateVariableSO candidateGameState = SaveLoadManager.LoadData();
                if (candidateGameState && resumeableLocations.Contains(candidateGameState.currentLocation)) {
                    replaceThisWith(candidateGameState);
                    LoadCurrentLocationScene();
                }*/
                break;
            case Location.INTRO_CUTSCENE:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.WAKE_UP_ROOM:
                Debug.Log("LEAVING WAKE UP ROOM");
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.TEAM_SIGNING:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.TUTORIAL:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            // map probably shouldn't stay its own location
            // but we'll need to force the player to choose the next destination at some point.
            // unsure
            case Location.MAP:
                Debug.Log("Leaving map, current location is: " + currentLocation + " next location is " + nextLocation);
                if(nextLocation == Location.NONE) {
                    Debug.LogError("A next location must be specified when leaving map");
                } else if (nextLocation == Location.COMBAT) {
                    currentLocation = Location.TEAM_SELECT;
                } else if (nextLocation == Location.SHOP) {
                    currentLocation = Location.SHOP;
                } else {
                    Debug.LogError("Invalid next location specified when leaving map");
                }
                break;
            case Location.TEAM_SELECT:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.PRE_COMBAT_SPLASH:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.COMBAT:
                Debug.Log("Leaving combat, current location is: " + currentLocation);
                currentEncounterIndex++;
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.POST_COMBAT:
                Debug.Log("Leaving post combat, current location is: " + currentLocation);
                if (!hasSeenTutorial) {
                    currentLocation = Location.SHOP_TUTORIAL;
                } else {
                    currentLocation = locationToNextLocation[currentLocation];
                    AdvanceEncounter();
                }
                break;
            case Location.SHOP_TUTORIAL:
                hasSeenTutorial = true;
                currentLocation = locationToNextLocation[currentLocation];
                AdvanceEncounter();
                break;
            case Location.SHOP:
                currentEncounterIndex++;
                currentLocation = locationToNextLocation[currentLocation];
                EndTutorialLoop();
                AdvanceEncounter();
                break;
            case Location.FAKE_SHOP:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.PRE_BOSSFIGHT_COMBAT_SPLASH:
                currentLocation = locationToNextLocation[currentLocation];
                break;
            default:
                Debug.Log("Invalid location in LoadNextLocation switch case");
                break;
        }
        updateMusic(currentLocation);
        cancelCurrentDialogue();
        LoadCurrentLocationScene();
        currentLoopIndex = GetLoopIndex();
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
            // TODO: call a scene transition handler
            SceneTransitionManager.LoadScene(locationToScene[currentLocation]);
            //SceneManager.LoadScene(locationToScene[currentLocation]);
        }
    }

    private void EndTutorialLoop() {
        if(GetLoopIndex() >= 2) {
            Debug.Log("Tutorial loop ended");
            playerData.GetValue().seenTutorial = true;
        }
    }

    // TODO: Change this logic if we want map choices to ever actually matter
    private void AdvanceEncounter() {
        activeEncounter.SetValue(nextEncounter.GetValue());
        Debug.Log("Advanced encounter, new encounter type is : " + activeEncounter.GetValue().getEncounterType());
        nextMapIndex += 1;
        if(nextMapIndex < map.GetValue().encounters.Count) {
            nextEncounter.SetValue(map.GetValue().encounters[nextMapIndex]);
        } else {
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
        Debug.Log("hovering or ending hovering");
        Debug.Log(companionInstance);
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
            MusicController2.Instance.PlayMusicLocation(newLocation);
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

    private void replaceThisWith(GameStateVariableSO sourceObject)
    {
        if (sourceObject == null)
        {
            Debug.LogError("Source object is null!");
            return;
        }

        // Manually copy each field

        this.companions = sourceObject.companions;
        this.playerData = sourceObject.playerData;
        this.map = sourceObject.map;
        this.baseShopData = sourceObject.baseShopData;
        this.mapGenerator = sourceObject.mapGenerator;
        this.activeEncounter = sourceObject.activeEncounter;
        this.nextMapIndex = sourceObject.nextMapIndex;
        this.currentLoopIndex = sourceObject.currentLoopIndex;
        this.nextEncounter = sourceObject.nextEncounter;
        this.currentLocation = sourceObject.currentLocation;
        this.dialogueLocations = sourceObject.dialogueLocations;
        this.debugSingleEncounterMode = sourceObject.debugSingleEncounterMode;
        this.viewedSequences = sourceObject.viewedSequences;
        this.hoveredCompanion = sourceObject.hoveredCompanion;
        this.currentEncounterIndex = sourceObject.currentEncounterIndex;
        this.hasSeenTutorial = sourceObject.hasSeenTutorial;
    }
}
