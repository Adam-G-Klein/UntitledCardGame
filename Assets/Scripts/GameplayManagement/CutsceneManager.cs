using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CutsceneManager : GenericSingleton<CutsceneManager> {
    [SerializeField]
    private GameStateVariableSO gameState;

    public void NextScene() {
        gameState.LoadNextLocation();
    }


}