using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestSetupCompanions : MonoBehaviour
{
    public GameStateVariableSO gameStateVariableSO;
    public ShopViewController shopViewController;

    [ContextMenu("Go")]
    public void Go() {
        shopViewController.SetupUnitManagement(gameStateVariableSO.companions);
    }

    // This function is called under the assumption that the companion in question
    // is NOT being swapped with another companion
    public void MoveCompanionToActiveAtIndex(Companion companion, int index) {
        if (gameStateVariableSO.companions.activeCompanions.Contains(companion)) {
            gameStateVariableSO.companions.activeCompanions.Remove(companion);
        } else if (gameStateVariableSO.companions.benchedCompanions.Contains(companion)) {
            gameStateVariableSO.companions.benchedCompanions.Remove(companion);
        }
        gameStateVariableSO.companions.activeCompanions.Add(companion);
    }

    // This function is called under the assumption that the companion in question
    // is NOT being swapped with another companion
    public void MoveCompanionToBenchAtIndex(Companion companion, int index) {
        if (gameStateVariableSO.companions.benchedCompanions.Contains(companion)) {
            gameStateVariableSO.companions.benchedCompanions.Remove(companion);
        } else if (gameStateVariableSO.companions.activeCompanions.Contains(companion)) {
            gameStateVariableSO.companions.activeCompanions.Remove(companion);
        }
        gameStateVariableSO.companions.benchedCompanions.Add(companion);
    }

    public void SetCompanionOrdering(List<Companion> activeCompanions, List<Companion> benchCompanions) {
        gameStateVariableSO.companions.activeCompanions = activeCompanions;
        gameStateVariableSO.companions.benchedCompanions = benchCompanions;
    }
}
