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
    [Header("Demo Build")]
    public MapGeneratorSO demoMapGenerator;
    public CompanionPoolSO demoBaseCompanionPool;
    public List<PackSO> demoActivePacks;

    public void generateMapAndChangeScenes() {
        if (gameState.BuildTypeDemoOrConvention()) {
            gameState.PopulateCompanionPool(demoBaseCompanionPool);
            gameState.activePacks = new List<PackSO>(demoActivePacks);
            gameState.StartNewRun(demoMapGenerator);
        } else {
            gameState.PopulateCompanionPool(baseCompanionPool);
            gameState.activePacks = new List<PackSO>(activePacks);
            gameState.StartNewRun(mapGenerator);
        }
    }

}
