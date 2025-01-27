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

    // AttemptCompanionUpgrade searches for enough other companions of the same type and level.
    // If there are enough at the threshold, it removes the existing companions and creates a
    // combined version to be returned by this method.
    // WARNING: side-effect-ey - this modifies the list of companions on the game state.
    public Companion AttemptCompanionUpgrade(Companion newGuy) {
        if (newGuy.companionType.upgradeTo == null) {
            Debug.LogError("purchased a companion and there is no level 2 for it");
            return null;
        }
        List<Companion> existingCompanions = CompanionsOfType(newGuy.companionType);
        existingCompanions.Add(newGuy);

        int numNeededToCombine = newGuy.companionType.level == CompanionLevel.LevelOne ?
            gameplayConstants.COMPANIONS_FOR_LEVELTWO_COMBINATION :
            gameplayConstants.COMPANIONS_FOR_LEVELTHREE_COMBINATION;

        // This is an expected result when you buy a companion and do not have enough.
        if (existingCompanions.Count < numNeededToCombine) {
            Debug.Log("Not enough companions to trigger combination!");
            return null;
        }

        Debug.Log("Combining companions of type " + newGuy.companionType.name + " at level " + newGuy.companionType.level.ToString());

        newGuy.InvokeOnCombineAbilities(gameState);
        Companion combined = CombineCompanions(existingCompanions);

        // Mutate the game state to remove the old ones.
        gameState.RemoveCompanionsFromTeam(existingCompanions);

        Instantiate(celebrationParticles, gameObject.transform.position, Quaternion.identity);
        return combined;
    }

    // CombineCompanions returns the new combined companion with its deck and stats.
    private Companion CombineCompanions(List<Companion> companions) {
        Deck combinedDeck = selectDeckForCombinedCompanions(companions);

        // TODO: change this combined companion with whatever mechanics ends up wanting
        Companion combinedCompanion = new Companion(companions[0].companionType.upgradeTo);
        combinedCompanion.deck = combinedDeck;
        combinedCompanion.combatStats.maxHealth = maxHealthForCombinedCompanion(companions);
        combinedCompanion.combatStats.currentHealth = (int) (combinedCompanion.combatStats.maxHealth * currentHealthPctForCombinedCompanion(companions));
        combinedCompanion.combatStats.baseAttackDamage = baseAttackDamageForCombinedCompanion(companions);

        return combinedCompanion;
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
        // Sum together the max health of each companion.
        return (int) companions.Select(c => c.combatStats.maxHealth).ToList().Sum();
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
