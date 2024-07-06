using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
TODO:
- implement rudimentary tooltip system?
- make the companion purchase button glow if the player has enough companions to combine

Tooltips:
- tooltip component, instantiate object that follows mouse on hover after a delay
- tooltip view, populates an attached textmeshpro with the string in the tooltip object
- tooltip object, serializeReference from TooltipInstantiator
*/
public class CompanionCombinationManager : MonoBehaviour
{
    [SerializeField]
    private GameStateVariableSO gameState;
    [SerializeField]
    private GameplayConstantsSO gameplayConstants;

    [SerializeField]
    private GameObject celebrationParticles;
     
    public bool AttemptUpgradeCompanion(Companion companion) {
        if(companion.companionType.upgradeTo == null) {
            Debug.LogError("Trying to upgrade companion, but no upgradeTo companionType set");
            return false;
        }
        List<Companion> companions = CompanionsOfType(companion.companionType);
        // Add the one that was just purchased
        companions.Add(companion);

        if(companions.Count >= gameplayConstants.COMPANIONS_FOR_COMBINATION) {
            CombineCompanions(companions);
            Instantiate(celebrationParticles, gameObject.transform.position, Quaternion.identity);
            return true;
        }
        Debug.Log("Not enough companions to combine, count: " + companions.Count + " needed: " + gameplayConstants.COMPANIONS_FOR_COMBINATION);
        return false;
    }

    private void CombineCompanions(List<Companion> companions) {
        if(companions.Count != gameplayConstants.COMPANIONS_FOR_COMBINATION) {
            Debug.LogError("Trying to combine companions, but not COMPANIONS_FOR_COMBINATION companions");
            return;
        }
        foreach(Companion c in companions) {
            if(gameState.companions.activeCompanions.Contains(c)) {
                gameState.companions.activeCompanions.Remove(c);
            }
            if(gameState.companions.benchedCompanions.Contains(c)) {
                gameState.companions.benchedCompanions.Remove(c);
            }
        }
        // TODO: change this combined companion with whatever mechanics ends up wanting
        Companion combinedCompanion = new Companion(companions[0].companionType.upgradeTo);
        if(gameState.companions.spaceInActiveCompanions) {
            gameState.companions.activeCompanions.Add(combinedCompanion);
        } else {
            gameState.companions.benchedCompanions.Add(combinedCompanion);
        }
    }

    private List<Companion> CompanionsOfType(CompanionTypeSO companionType) {
        return gameState.companions.allCompanions.FindAll(c => c.companionType == companionType);
    }
}
