using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Location {
    NONE,
    MAIN_MENU,
    WAKE_UP_ROOM,
    TEAM_SIGNING,
    TUTORIAL,
    MAP,
    TEAM_SELECT,
    PRE_COMBAT_SPLASH,
    COMBAT,
    POST_COMBAT,
    SHOP,
    FAKE_SHOP,
    PRE_BOSSFIGHT_COMBAT_SPLASH,
    BOSSFIGHT
}
/*

team-selection
pre-combat splash
combat
post-combat (could just be a popup over the enemy combat)
post-combat-map (Probably clunky to implement the map as a separate scene)
shop
pre-bossfight-combat splash (fun idea of non-skippable villain monologue)

*/
[CreateAssetMenu(
    fileName = "NewGameStateVariable",
    menuName = "Variables/Game State Variable")]
public class GameStateVariableSO : ScriptableObject
{
    public CompanionListVariableSO companions;
    public PlayerDataVariableSO playerData;
    public MapVariableSO map;
    [Header("The Combat or Shop Encounter we're currently in")]
    public EncounterVariableSO activeEncounter;
    [Header("The Next Combat or Shop Encounter we'll enter")]
    public EncounterVariableSO nextEncounter;
    public Location currentLocation;
    private Dictionary<Location, string> locationToScene = new Dictionary<Location, string>() {
        {Location.MAIN_MENU, "MainMenu"},
        {Location.WAKE_UP_ROOM, "AidensRoom"},
        {Location.TEAM_SIGNING, "TeamSigning"},
        {Location.TUTORIAL, "PlaceholderEnemyEncounter"},
        {Location.MAP, "Map"},
        {Location.TEAM_SELECT, "TeamSelect"},
        {Location.PRE_COMBAT_SPLASH, "PreCombatSplash"},
        {Location.COMBAT, "PlaceholderEnemyEncounter"},
        {Location.POST_COMBAT, "PlaceholderEnemyEncounter"},
        {Location.SHOP, "PlaceholderShopEncounter"},
        // not implemented yet
        {Location.FAKE_SHOP, "FakeShop"},
        {Location.PRE_BOSSFIGHT_COMBAT_SPLASH, "PreBossfightCombatSplash"},
        {Location.BOSSFIGHT, "PlaceholderBossfightEncounter"}
    };

    private Dictionary<Location, Location> locationToNextLocation = new Dictionary<Location, Location>() {
        {Location.MAIN_MENU, Location.WAKE_UP_ROOM},
        {Location.WAKE_UP_ROOM, Location.TEAM_SIGNING},
        {Location.TEAM_SIGNING, Location.TUTORIAL},
        {Location.TUTORIAL, Location.POST_COMBAT},
        {Location.TEAM_SELECT, Location.PRE_COMBAT_SPLASH},
        {Location.PRE_COMBAT_SPLASH, Location.COMBAT},
        {Location.COMBAT, Location.POST_COMBAT},
        {Location.POST_COMBAT, Location.MAP},
        {Location.SHOP, Location.MAP},
        {Location.FAKE_SHOP, Location.PRE_BOSSFIGHT_COMBAT_SPLASH},
        {Location.PRE_BOSSFIGHT_COMBAT_SPLASH, Location.BOSSFIGHT},
        {Location.BOSSFIGHT, Location.MAP}
    };

    public int nextMapIndex = 0;

    // TODO: make this more versatile if we want the map to actually do things.
    // also want to field criticism about whether this should live here.
    // My only argument for placing it here is that this is one of the main 
    // functions of the GameStateVariable, and the logic is based entirely on 
    // what our current game state is. Lmk your thoughts though.
    public void LoadNextLocation(Location nextLocation = Location.NONE) { 
        Debug.Log("Loading next location, current location is: " + currentLocation);
        // Thanks for genning this copilot <3 :)
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
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.WAKE_UP_ROOM:
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
                currentLocation = locationToNextLocation[currentLocation];
                break;
            case Location.POST_COMBAT:
                Debug.Log("Leaving post combat, current location is: " + currentLocation);
                currentLocation = locationToNextLocation[currentLocation];
                AdvanceEncounter();
                break;
            case Location.SHOP:
                currentLocation = locationToNextLocation[currentLocation];
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
        LoadCurrentLocationScene();
    }

    public void LoadCurrentLocationScene() {
        if(!locationToScene.ContainsKey(currentLocation)) {
            Debug.LogError("ERROR: Invalid location");
            return;
        }
        if(locationToScene[currentLocation] != SceneManager.GetActiveScene().name) {
            // TODO: call a scene transition handler
            SceneManager.LoadScene(locationToScene[currentLocation]);
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

    public void SetLocation(Location newLocation) {
        currentLocation = newLocation;
    }

}
