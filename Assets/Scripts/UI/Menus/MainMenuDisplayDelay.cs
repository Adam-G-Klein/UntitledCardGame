
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
    private bool skipped = false;

    void Start() {
        StartCoroutine(DelayMainMenuDisplay());
        gameState.SetLocation(Location.MAIN_MENU);
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.J) && gameState.currentLocation == Location.MAIN_MENU){
            generateMap.generateMapAndChangeScenes();
            skipped = true;
        }
    }

    private IEnumerator DelayMainMenuDisplay() {
        yield return new WaitForSecondsRealtime(delayTime);
        if(!skipped) {
            mainMenu.SetActive(true);
            mainMenu.GetComponent<CanvasShaker>().ScreenShakeForTime(0.25f, mainMenu.gameObject.GetComponent<Canvas>());
        }
        yield return null;
    }

    private void shakeCanvas(Canvas canvas) {
        mainMenu.GetComponent<CanvasScaleAnimator>().scaleForTime(1, 0.9f, (Canvas canvas) => shakeCanvas(canvas));
    }
}