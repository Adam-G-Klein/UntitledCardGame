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

    private IEnumerator AnimateProgressBarWithDelay(ProgressBarView progressBar, int newVal, float delay) {
        yield return new WaitForSeconds(delay);
        progressBar.AnimateProgressBar(newVal);
    }
    
}