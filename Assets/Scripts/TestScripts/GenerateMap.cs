using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/*

Putting this in the main menu play button for now, I figure it may have to move a couple more times
before it finds its final resting place

*/

public class GenerateMap : MonoBehaviour
{
    public GameStateVariableSO gameState;
    public MapGeneratorSO mapGenerator;

    public void generateMapAndChangeScenes() {
        gameState.map.SetValue(mapGenerator.generateMap());
        gameState.playerData.initializeRun();
        gameState.SetLocation(Location.MAIN_MENU);
        gameState.LoadNextLocation();
    }

}
