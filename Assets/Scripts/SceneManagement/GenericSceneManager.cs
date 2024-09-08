using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSceneManager : MonoBehaviour
{
    public GameStateVariableSO gameState;

    public bool allowProgressingGameState;
    public GameObject skipTooltip;

    private bool showingSkipTooltipAlready = false;

    public void Update() {
        if (allowProgressingGameState && Input.GetMouseButtonDown(0)) {
            StartCoroutine(ShowSkipTooltip());
        }

        if (allowProgressingGameState && Input.GetKeyDown(KeyCode.S)) {
            ContinueGameState();
        }
    }

    public void ContinueGameState() {
        gameState.LoadNextLocation();
    }

    private IEnumerator ShowSkipTooltip() {
        if (showingSkipTooltipAlready) {
            yield break;
        }
        showingSkipTooltipAlready = true;

        skipTooltip.SetActive(true);
        yield return new WaitForSeconds(2);
        skipTooltip.SetActive(false);

        showingSkipTooltipAlready = false;
    }
}
