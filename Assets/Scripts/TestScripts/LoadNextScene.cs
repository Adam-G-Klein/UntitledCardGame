using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadNextScene: MonoBehaviour
{
    public GameStateVariableSO gameState;

    public void loadNextScene() {
        gameState.LoadNextLocation();
    }
}
