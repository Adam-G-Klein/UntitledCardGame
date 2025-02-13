using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSceneManager : MonoBehaviour
{
    public GameStateVariableSO gameState;

    public bool allowProgressingGameState;
    public GameObject skipTooltip;
    public int skipClicks = 3;

    private bool showingSkipTooltipAlready = false;
    private bool checkingForSkipClicksAlready = false;

    private int skipCounter = 0;

    public void Update() {
        if (allowProgressingGameState && Input.GetMouseButtonDown(0)) {
            // StartCoroutine(ShowSkipTooltip());
            if (!checkingForSkipClicksAlready) {
                skipCounter = 1;
                StartCoroutine(PossiblyShowSkipTooltip());
                checkingForSkipClicksAlready = true;
            } else {
                skipCounter += 1;
            }
        }

        if (allowProgressingGameState && Input.GetKeyDown(KeyCode.J)) {
            ContinueGameState();
        }
    }

    public void ContinueGameState() {
        gameState.LoadNextLocation();
    }

    private IEnumerator PossiblyShowSkipTooltip() {
        yield return new WaitForSeconds(1f);
        if (skipCounter >= skipClicks) {
            StartCoroutine(ShowSkipTooltip());
        }
        checkingForSkipClicksAlready = false;
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
