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

    public bool AttemptUpgradeCompanion(Companion purchasedCompanion) {

        List<Companion> companions = CompanionsOfType(purchasedCompanion.companionType);
        // Add the one that was just purchased
        companions.Add(purchasedCompanion);

        if(companions.Count >= gameplayConstants.COMPANIONS_FOR_COMBINATION) {
            if(purchasedCompanion.companionType.upgradeTo == null) {
                Debug.LogWarning("Attempting to upgrade companion, but no upgradeTo companionType set");
                return false;
            }
            // trigger the ability before combination, all the information about combined
            // companions will still be readily available that way
            purchasedCompanion.InvokeOnCombineAbilities(gameState);
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

        Deck combinedDeck = selectDeckForCombinedCompanions(companions);

        // TODO: change this combined companion with whatever mechanics ends up wanting
        Companion combinedCompanion = new Companion(companions[0].companionType.upgradeTo);
        combinedCompanion.deck = combinedDeck;
        combinedCompanion.deck.cardsDealtPerTurn = companions[0].companionType.upgradeTo.initialCardsDealtPerTurn;

        combinedCompanion.combatStats.maxHealth = maxHealthForCombinedCompanion(companions);
        combinedCompanion.combatStats.currentHealth = (int) (combinedCompanion.combatStats.maxHealth * currentHealthPctForCombinedCompanion(companions));
        combinedCompanion.combatStats.baseAttackDamage = baseAttackDamageForCombinedCompanion(companions);

        if(gameState.companions.spaceInActiveCompanions) {
            gameState.companions.activeCompanions.Add(combinedCompanion);
        } else {
            gameState.companions.benchedCompanions.Add(combinedCompanion);
        }
    }

    private List<Companion> CompanionsOfType(CompanionTypeSO companionType) {
        return gameState.companions.allCompanions.FindAll(c => c.companionType == companionType);
    }

    private Deck selectDeckForCombinedCompanions(List<Companion> companions) {
        // TODO(): Allow the player to choose which deck they want to use.
        // For now, select the largest deck, because presumably that will contain
        // the most cards the player bought.
        // It should be a good heuristic for now.
        Deck selectedDeck = companions[0].deck;
        for (int i = 1; i < companions.Count; i++) {
            if (companions[i].deck.cards.Count > selectedDeck.cards.Count) {
                selectedDeck = companions[i].deck;
            }
        }
        return selectedDeck;
    }

    private int maxHealthForCombinedCompanion(List<Companion> companions) {
        // Average together the max health of each companion.
        return (int) companions.Select(c => c.combatStats.maxHealth).ToList().Average();
    }

    private double currentHealthPctForCombinedCompanion(List<Companion> companions) {
        // Average together the current health % of each companion.
        return companions.Select(c => ((double) c.combatStats.currentHealth) / c.combatStats.maxHealth).ToList().Average();
    }

    private int baseAttackDamageForCombinedCompanion(List<Companion> companions) {
        // Sum up the base attack damages of each companion.
        return companions.Select(c => c.combatStats.baseAttackDamage).ToList().Sum();
    }
}
