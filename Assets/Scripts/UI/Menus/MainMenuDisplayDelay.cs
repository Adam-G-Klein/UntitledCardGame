
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuDisplayDelay : MonoBehaviour 
{
    public GameObject mainMenu;
    public float delayTime;
    public GameStateVariableSO gameState;
    public GenerateMap generateMap;

    void Start() {
        StartCoroutine(DelayMainMenuDisplay());
        gameState.SetLocation(Location.MAIN_MENU);
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.S) && gameState.currentLocation == Location.MAIN_MENU){
            generateMap.generateMapAndChangeScenes();
        }
    }

    private IEnumerator DelayMainMenuDisplay() {
        yield return new WaitForSeconds(delayTime);
        mainMenu.SetActive(true);
    }
}