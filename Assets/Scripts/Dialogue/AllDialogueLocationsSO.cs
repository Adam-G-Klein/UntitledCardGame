using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(
    fileName = "NewDialogueLocationList", 
    menuName = "Dialogue/Dialogue Location List")]
[System.Serializable]
public class AllDialogueLocationsSO: ScriptableObject
{
    public List<DialogueLocationSO> aidenRooms;
    public List<DialogueLocationSO> teamSelectionScreens;
    public List<DialogueLocationSO> preCombatScreens;
    public List<DialogueLocationSO> shops;
    public List<DialogueLocationSO> combats;
    public List<DialogueLocationSO> postCombats;

    [Header("Tutorial sequences. All companion lines optional")]
    public DialogueLocationSO tutorialAidensRoom;
    public DialogueLocationSO tutorialTeamSigning;
    public DialogueLocationSO tutorialMap1;
    public DialogueLocationSO tutorialTeamSelect1;
    public DialogueLocationSO tutorialPrecombatSplash1;
    public DialogueLocationSO tutorialCombat1;
    public DialogueLocationSO tutorialPostCombat1;
    public DialogueLocationSO tutorialShop1;
    public DialogueLocationSO tutorialMap2;
    public DialogueLocationSO tutorialPrecombatSplash2;
    public DialogueLocationSO tutorialCombat2;
    public DialogueLocationSO tutorialPostCombat2;
    public DialogueLocationSO tutorialShop2;

    [Header("Boss fight sequences (not implemented yet)")]
    public DialogueLocationSO bossFightShop;
    public DialogueLocationSO bossFightPrecombatSplash;
    public List<DialogueSequenceSO> bossFightCombat;

    public DialogueLocationSO GetDialogueLocation(GameStateVariableSO gameState) {
        int loopIndex = gameState.GetLoopIndex();
        Location loc = gameState.currentLocation;
        bool seenTutorial = gameState.playerData.GetValue().seenTutorial;
        bool inTutorialRun = gameState.playerData.GetValue().inTutorialRun;
        Debug.Log("Getting dialogue location for " + loc + " loop index " + loopIndex + " seen tutorial " + gameState.playerData.GetValue().seenTutorial);
        if(seenTutorial && !inTutorialRun) {
            return GetDialogueLocationAtIndex(loc, loopIndex);
        } else if (inTutorialRun && seenTutorial){
            // add two loops for the tuto
            return GetDialogueLocationAtIndex(loc, loopIndex - 2);
        }
        else if (!seenTutorial && inTutorialRun) {
            return GetTutorialLocation(gameState);
        } else {
            Debug.LogError("Player has not seen tutorial and is not in tutorial run, wrong dialogue will display");
            Debug.LogError("Current location: " + loc);
            Debug.LogError("seen tutorial: " + gameState.playerData.GetValue().seenTutorial);
            Debug.LogError("in tutorial run: " + gameState.playerData.GetValue().inTutorialRun);
            Debug.LogError("loop index: " + loopIndex);
            return GetDialogueLocationAtIndex(loc, loopIndex);
        }
        
    }

    private DialogueLocationSO GetDialogueLocationAtIndex(Location loc, int index) {
        List<DialogueLocationSO> locList = GetDialogueLocationList(loc);
        if(index >= locList.Count || index < 0) {
            Debug.LogWarning("Location index " + index + " is out of range for location " + loc + ", just using the last one");
            if(locList.Count == 0) {
                Debug.LogWarning("Location " + loc + " has no dialogue locations");
                return null;
            }
            return locList.Last();
        } else {
            return locList[index];
        }
    }
    // Lemme know if you can think of a cleaner way to do this
    // static dictionary doesn't work because we want the list from the specific instance
    private List<DialogueLocationSO> GetDialogueLocationList(Location loc) {
        switch(loc) {
            case Location.WAKE_UP_ROOM:
                return aidenRooms;
            // TODO implement team signing for after revival
            case Location.PRE_COMBAT_SPLASH:
                return preCombatScreens;
            case Location.TEAM_SELECT:
                return teamSelectionScreens;
            case Location.SHOP:
                return shops;
            case Location.COMBAT:
                return combats;
            case Location.POST_COMBAT:
                return postCombats;
            default:
                Debug.LogError("Location " + loc + " not found in AllDialogueLocationsSO");
                return null;
        }
    }

    // loop index is from gameState.GetLoopIndex()
    private DialogueLocationSO GetTutorialLocation(GameStateVariableSO gameState) {
        Location loc = gameState.currentLocation;
        int lastTutorialCombatLoop = gameState.lastTutorialLoopIndex;
        int lastTutorialShop = gameState.lastTutorialLoopIndex + 1;
        int loopIndex = gameState.GetLoopIndex();
        /*
        loop index to tutorial location mapping:
        0 -> tutorialAidensRoom
        0 -> tutorialTeamsigning
        0,1 -> precombatSplash
        0,1 -> combat
        1,2 -> postcombat
        1,2 -> shop
        1,2 -> tutorialTeamselect

        */
        switch(loc) {
            case Location.WAKE_UP_ROOM:
                return tutorialAidensRoom;
            case Location.STARTING_TEAM:
                return tutorialTeamSigning;
            case Location.TEAM_SELECT:
                return tutorialTeamSelect1;
            case Location.PRE_COMBAT_SPLASH:
                return selectTutorialLocOnLoopIndex(loopIndex, lastTutorialCombatLoop, tutorialPrecombatSplash1, tutorialPrecombatSplash2);
            case Location.COMBAT:
                return selectTutorialLocOnLoopIndex(loopIndex, lastTutorialCombatLoop, tutorialCombat1, tutorialCombat2);
            case Location.POST_COMBAT:
                return selectTutorialLocOnLoopIndex(loopIndex, lastTutorialCombatLoop, tutorialPostCombat1, tutorialPostCombat2);
            // AdvanceEncounter is called after postcombat
            case Location.MAP:
                return selectTutorialLocOnLoopIndex(loopIndex, lastTutorialShop, tutorialMap1, tutorialMap2);
            case Location.SHOP:
                return selectTutorialLocOnLoopIndex(loopIndex, lastTutorialShop, tutorialShop1, tutorialShop2);
            default:
                Debug.LogError("Location " + loc + " not found in AllDialogueLocationsSO");
                return null;
                
        }
    }

    private DialogueLocationSO selectTutorialLocOnLoopIndex(int loopIndex, int lastTutorialLoopIndex, DialogueLocationSO loc1, DialogueLocationSO loc2) {
        if(lastTutorialLoopIndex - loopIndex == 1) {
            return loc1;
        } else if (lastTutorialLoopIndex - loopIndex == 0) {
            return loc2;
        } else {
            Debug.LogError("Loop index " + loopIndex + " is not 1 or 0 away from last tutorial loop index " + lastTutorialLoopIndex);
            return loc1;
        }
    }

}