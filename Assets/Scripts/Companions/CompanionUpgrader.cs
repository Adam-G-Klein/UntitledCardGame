using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CompanionUpgrader 
{
    public static int companionsNeededToCombine = 3;

    //send to the bench for now
    public static void Combine(List<Companion> activeCompanions,
                                    List<Companion> benchCompanions,
                                    List<Companion> companionsToCombine) {
        if ((companionsToCombine.Count != companionsNeededToCombine) || (!areCompanionsTheSame(companionsToCombine))) {
            //silently fail for now lmao
            Debug.Log("Different types of companions cannot be combined, or the number to select is incorrect.");
            return;
        }

        removeCompanionsToCombine(activeCompanions, benchCompanions, companionsToCombine);

        //now that the companions are out of the list, go ahead and select the first companions as the blueprint
        Companion companion = new Companion(companionsToCombine[0].companionType);
        
        companion.Upgrade(companionsToCombine);

        //add the upgraded companion back to the benched companions
        benchCompanions.Add(companion);
    }

    private static bool areCompanionsTheSame(List<Companion> companionsToCombine) {
        var lastCompanion = companionsToCombine[0].companionType;

        for (int i = 1; i < companionsToCombine.Count; i++) {

            var currentCompanion = companionsToCombine[i].companionType;
            if (lastCompanion != currentCompanion) {
                return false;
            }

            lastCompanion = currentCompanion;
        }
        
        return true;
    }

    private static void removeCompanionsToCombine(List<Companion> activeCompanions,
                                            List<Companion> benchCompanions,
                                            List<Companion> companionsToCombine) {
        //clear the activeCompanions
        foreach(var companionToCombine in companionsToCombine) {
            if (activeCompanions.Contains(companionToCombine)) {
                activeCompanions.Remove(companionToCombine);
            }
            else if (benchCompanions.Contains(companionToCombine)) {
                benchCompanions.Remove(companionToCombine);
            }
        }
    }


}
