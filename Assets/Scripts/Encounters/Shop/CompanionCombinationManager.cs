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

    public List<Companion> PurchaseWouldCauseUpgrade(Companion newGuy) {
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

        // Getting here means that we would get at least one upgrade triggered. We need to see if multiple would trigger
        if (newGuy.companionType.level == CompanionLevel.LevelOne) {
            Companion combined = ShowUpgradedCompanion(existingCompanions); // this creates a temp upgrade companion to see if another combination would trigger
            List<Companion> secondUpgradeCompanions = PurchaseWouldCauseUpgrade(combined);
            if (secondUpgradeCompanions != null) existingCompanions.Add(secondUpgradeCompanions[0]); // this is a little hacky/implementation dependent
        }

        return existingCompanions;
    }

    public Companion ShowUpgradedCompanion(List<Companion> companions) {
        return CombineCompanions(companions);
    }

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
        combinedCompanion.combatStats.maxHealth = maxHealthForCombinedCompanion(combinedCompanion, companions);
        combinedCompanion.combatStats.currentHealth = (int) (combinedCompanion.combatStats.maxHealth * currentHealthPctForCombinedCompanion(companions));
        combinedCompanion.combatStats.baseAttackDamage = baseAttackDamageForCombinedCompanion(companions);

        return combinedCompanion;
    }

    private List<Companion> CompanionsOfType(CompanionTypeSO companionType) {
        return gameState.companions.allCompanions.FindAll(c => c.companionType == companionType);
    }

    private Deck selectDeckForCombinedCompanions(List<Companion> companions) {
        // TODO(): Allow the player to choose which deck they want to use.
        // For now, select the deck with the least starting cards and add all added cards from other decks


        // find deck with the least starting cards.

        Companion companionWithLeastStartingCards = null;
        int leastStartingCards = 10000;

        for (int i = 0; i < companions.Count; i++) {
            int startingCards = 0;
            for (int j = 0; j < companions[i].deck.cards.Count; j++) {
                if (companions[i].companionType.startingDeck.cards.Contains(companions[i].deck.cards[j].cardType)) {
                    startingCards += 1;
                }
            }
            if (startingCards < leastStartingCards) {
                leastStartingCards = startingCards;
                companionWithLeastStartingCards = companions[i];
            }
        }


        // Add all non starting cards from other decks and all cards from the chosen deck to a new deck!
        List<Card> cards = new();

        for (int i = 0; i < companions.Count; i++) {
            for (int j = 0; j < companions[i].deck.cards.Count; j++) {
                if (companions[i].id == companionWithLeastStartingCards.id || !companions[i].companionType.startingDeck.cards.Contains(companions[i].deck.cards[j].cardType)) {
                    cards.Add(companions[i].deck.cards[j]);
                }
            }
        }

        // Remove one card from the base deck each time you upgrade.
        // This is a great experimental idea from Ethan that we should try.
        cards.Shuffle();
        for (int i = 0; i < cards.Count; i++) {
            if (gameState.baseShopData.baseCardsToRemoveOnUpgrade.Contains(cards[i].cardType)) {
                cards.RemoveAt(i);
                break;
            }
        }

        return new Deck(cards);
    }

    private int maxHealthForCombinedCompanion(Companion upgradedCompanion, List<Companion> companions) {
        // Take the base health of the upgraded companion, then
        // add any accumulated max HP buffs.
        int maxHealthBuffs = (int) companions.Select(c => c.combatStats.getMaxHealthBuffs()).ToList().Sum();
        return upgradedCompanion.companionType.maxHealth + maxHealthBuffs;
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
