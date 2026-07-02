using System.Collections;
using System.Collections.Generic;
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

    public void generateMapAndChangeScenes() {
        if (gameState.BuildTypeDemoOrConvention()) {
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

}
