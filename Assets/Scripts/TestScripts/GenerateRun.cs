using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
/*

Putting this in the main menu play button for now, I figure it may have to move a couple more times
before it finds its final resting place

*/

public class GenerateRun : MonoBehaviour
{
    public GameStateVariableSO gameState;
    [Header("Release Build")]
    public MapGeneratorSO mapGenerator;
    public CompanionPoolSO baseCompanionPool;
    public List<PackSO> activePacks;
    public int startingTeamSize;
    public int numStartingRats;
    public int startingGold;
    [Header("Release Build - first run (demo-style tutorial map)")]
    public MapGeneratorSO tutorialMapGenerator;
    [Header("Demo Build")]
    public MapGeneratorSO demoMapGenerator;
    public CompanionPoolSO demoBaseCompanionPool;
    public List<PackSO> demoActivePacks;
    public List<CompanionTypeSO> demoStaticCompanions;
    public int demoStartingTeamSize;
    public int demoNumStartingRats;
    public int demoStartingGold;
    [Header("Card Test Build")]
    public MapGeneratorSO cardTestMapGenerator;
    public List<CompanionTypeSO> testStaticCompanions;
    public StartingDeckSO testOverrideStartingDeck;
    public int testTeamSize;
    public int testStartingGold;

    public void generateMapAndChangeScenes() {
        if (gameState.buildType == BuildType.CARD_TEST) {
            initializeCardTestRun();
            gameState.StartNewRun(cardTestMapGenerator);
        } else if (gameState.BuildTypeDemoOrConvention()) {
            initializeDemoStyleRun();
            gameState.StartNewRun(demoMapGenerator);
        } else if (!gameState.HasCompletedTutorialRun && !gameState.skipTutorials) {
            // First release run: static tutorial map whose act one is the demo experience
            initializeDemoStyleRun();
            gameState.playerData.GetValue().isTutorialMapRun = true;
            // Once the demo portion is beaten, shops draw from the full game's pool/packs
            gameState.postTutorialCompanionPool = baseCompanionPool;
            gameState.postTutorialActivePacks = new List<PackSO>(activePacks);
            gameState.StartNewRun(tutorialMapGenerator);
        } else {
            gameState.PopulateCompanionPool(baseCompanionPool);
            gameState.activePacks = new List<PackSO>(activePacks);
            gameState.playerData.initialize(startingGold);
            gameState.playerData.GetValue().teamSize = startingTeamSize;
            gameState.playerData.GetValue().numStartingRats = numStartingRats;
            gameState.StartNewRun(mapGenerator);
        }
    }

    private void initializeDemoStyleRun() {
        gameState.PopulateCompanionPool(demoBaseCompanionPool);
        gameState.activePacks = new List<PackSO>(demoActivePacks);
        gameState.companions.respawn(demoStartingTeamSize);
        demoStaticCompanions.ForEach((companion) => gameState.companions.activeCompanions.Add(new Companion(companion)));
        gameState.playerData.initialize(demoStartingGold);
        gameState.playerData.GetValue().teamSize = demoStartingTeamSize;
        gameState.playerData.GetValue().numStartingRats = demoNumStartingRats;
    }

    private void initializeCardTestRun() {
        gameState.companions.respawn(testTeamSize);
        foreach (CompanionTypeSO companionType in testStaticCompanions) {
            Companion companion = new Companion(companionType);
            companion.deck = new Deck(
                testOverrideStartingDeck.cards
                    .Select(cardType => new Card(cardType, companionType, isStartingDeckCard: true))
                    .ToList(),
                companionType.initialCardsDealtPerTurn);
            gameState.companions.activeCompanions.Add(companion);
        }
        gameState.playerData.initialize(testStartingGold);
        gameState.playerData.GetValue().teamSize = testTeamSize;
        gameState.playerData.GetValue().numStartingRats = testStaticCompanions.Count;
    }

}
