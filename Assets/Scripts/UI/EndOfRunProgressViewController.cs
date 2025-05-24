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

    public UIDocument endOfRunProgressUIDocument;
    private VisualElement progressBarsContainer;
    public VisualTreeAsset progressBarTemplate;
    void Start()
    {
        progressBarsContainer = endOfRunProgressUIDocument.rootVisualElement.Q<VisualElement>("progressBarsContainer");
        progressBarsContainer.Clear();
        SetupProgressBars();

        Debug.LogError("setting up button");
        Button button = endOfRunProgressUIDocument.rootVisualElement.Q<Button>("exitButton");
        button.pickingMode = PickingMode.Position;
        button.RegisterOnSelected(() => {
            Debug.LogError("button clicked");
            button.SetEnabled(false);
            SceneManager.LoadScene("MainMenu");
        });
        FocusManager.Instance.RegisterFocusableTarget(button.AsFocusable());
    }

    private void SetupProgressBars()
    {
        float delay = 0f;
        foreach (var achievementSO in ProgressManager.Instance.achievementSOList)
        {
            if (achievementSO.lockedInProgress >= achievementSO.target) continue;

            ProgressBarView progressBar = new ProgressBarView(progressBarTemplate, achievementSO);
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