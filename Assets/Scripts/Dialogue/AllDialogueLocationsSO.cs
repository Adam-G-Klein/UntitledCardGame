using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(
    fileName = "NewDialogueLocationList", 
    menuName = "Dialogue/Dialogue Location List")]
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
    public DialogueLocationSO tutorialTeamSelect2;
    public DialogueLocationSO tutorialPrecombatSplash2;
    public DialogueLocationSO tutorialCombat2;
    public DialogueLocationSO tutorialPostCombat2;
    public DialogueLocationSO tutorialShop2;

    [Header("Boss fight sequences (not implemented yet)")]
    public DialogueLocationSO bossFightShop;
    public DialogueLocationSO bossFightPrecombatSplash;
    public List<DialogueSequenceSO> bossFightCombat;

    public DialogueLocationSO GetDialogueLocation(Location loc, int loopIndex, bool seenTutorial) {
        Debug.Log("Getting dialogue location for " + loc + " loop index " + loopIndex + " seen tutorial " + seenTutorial);
        if(seenTutorial) {
            List<DialogueLocationSO> locList = GetDialogueLocationList(loc);
            if(loopIndex >= locList.Count) {
                Debug.LogWarning("Location index " + loopIndex + " is out of range for location " + loc + ", just using the last one");
                return locList.Last();
            } else {
                return locList[loopIndex];
            }
        } else {
            return GetTutorialLocation(loc, loopIndex);
        }
        
    }
    // Lemme know if you can think of a cleaner way to do this
    // static dictionary doesn't work because we want the list from the specific instance
    private List<DialogueLocationSO> GetDialogueLocationList(Location loc) {
        switch(loc) {
            case Location.WAKE_UP_ROOM:
                return aidenRooms;
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
    private DialogueLocationSO GetTutorialLocation(Location loc, int loopIndex) {
        switch(loc) {
            case Location.WAKE_UP_ROOM:
                return tutorialAidensRoom;
            case Location.TEAM_SIGNING:
                return tutorialTeamSigning;
            case Location.MAP:
                if(loopIndex == 0) {
                    return tutorialMap1;
                } else {
                    return tutorialMap2;
                }
            case Location.TEAM_SELECT:
                if(loopIndex == 0) {
                    return tutorialTeamSelect1;
                } else {
                    return tutorialTeamSelect2;
                }
            case Location.PRE_COMBAT_SPLASH:
                if(loopIndex == 0) {
                    return tutorialPrecombatSplash1;
                } else {
                    return tutorialPrecombatSplash2;
                }
            case Location.COMBAT:
                if(loopIndex == 0) {
                    return tutorialCombat1;
                } else {
                    return tutorialCombat2;
                }
            case Location.POST_COMBAT:
                if(loopIndex == 0) {
                    return tutorialPostCombat1;
                } else {
                    return tutorialPostCombat2;
                }
            case Location.SHOP:
                if(loopIndex == 0) {
                    return tutorialShop1;
                } else {
                    return tutorialShop2;
                }
            default:
                return GetDialogueLocation(loc, loopIndex, true);
        }
    }

    

}