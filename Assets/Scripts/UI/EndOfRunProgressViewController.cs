using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;


public class EndOfRunProgressViewController : MonoBehaviour
{
    public GameStateVariableSO gameState;
    public UIDocument endOfRunProgressUIDocument;
    private VisualElement progressBarsContainer;
    public VisualTreeAsset progressBarTemplate;

    [Header("Card Unlock VFX")]
    [Tooltip("Plays over the screen when cards were unlocked this run. Leave empty for no effect.")]
    [SerializeField] private GameObject cardsUnlockedVFXPrefab;
    [Tooltip("Size multiplier on top of fitting the effect to the screen.")]
    [SerializeField] private float cardsUnlockedVFXScale = 1f;

    // VFX are authored against the standard orthographic camera size, so the effect is
    // scaled by this scene's camera size to cover the screen the same way.
    private const float VFX_REFERENCE_ORTHO_SIZE = 5f;

    void Start()
    {
        progressBarsContainer = endOfRunProgressUIDocument.rootVisualElement.Q<VisualElement>("progressBarsContainer");
        progressBarsContainer.Clear();
        SetupProgressBars();
        Button button = endOfRunProgressUIDocument.rootVisualElement.Q<Button>("exitButton");
        button.pickingMode = PickingMode.Position;
        button.RegisterOnSelected(() =>
        {
            button.SetEnabled(false);
            SceneTransitionManager.LoadScene("MainMenu", .5f);
        });
        if (gameState.buildType == BuildType.DEMO)
            endOfRunProgressUIDocument.rootVisualElement.Q<VisualElement>("demo-no-progress").style.display = DisplayStyle.Flex;
        FocusManager.Instance.RegisterFocusableTarget(button.AsFocusable());

        // Display cards unlocked in this run
        PlayCardsUnlockedVFX();

        // Save player progress after the new lockedInProgress values are set
        SaveManager.Instance.SavePlayerProgress();
    }

    private void SetupProgressBars()
    {
        float delay = 0f;
        foreach (var achievementSO in ProgressManager.Instance.achievementSOList)
        {
            if (achievementSO.lockedInProgress >= achievementSO.target) continue;

            ProgressBarView progressBar = new(progressBarTemplate, achievementSO);
            progressBarsContainer.Add(progressBar.root);

            StartCoroutine(AnimateProgressBarWithDelay(progressBar, achievementSO.currentProgress, delay));
            delay += 1f;
            achievementSO.lockedInProgress = achievementSO.currentProgress;
        }
    }

    private void PlayCardsUnlockedVFX()
    {
        if (cardsUnlockedVFXPrefab == null) return;
        List<CardType> unlocked = ProgressManager.Instance.progressSO.cardsUnlockedThisRun;
        if (unlocked == null || unlocked.Count == 0) return;

        Camera cam = Camera.main;
        Vector3 position = cam != null ? new Vector3(cam.transform.position.x, cam.transform.position.y, 0f) : Vector3.zero;
        float screenScale = (cam != null && cam.orthographic) ? cam.orthographicSize / VFX_REFERENCE_ORTHO_SIZE : 1f;

        GameObject vfx = Instantiate(cardsUnlockedVFXPrefab, position, Quaternion.identity);
        vfx.ScaleSelfAndChildren(screenScale * cardsUnlockedVFXScale);
    }

    private IEnumerator AnimateProgressBarWithDelay(ProgressBarView progressBar, int newVal, float delay) {
        yield return new WaitForSeconds(delay);
        progressBar.AnimateProgressBar(newVal);
    }
    
}