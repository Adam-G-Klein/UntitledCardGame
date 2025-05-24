using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;


public class ProgressBarView : MonoBehaviour
{

    private VisualTreeAsset progressBarTemplate;
    private AchievementSO achievementSO;
    public VisualElement root;
    private int startVal;
    private Label goalCurrentValue;
    private ProgressBar progressBar;
    public ProgressBarView(VisualTreeAsset progressBarTemplate, AchievementSO achievementSO)
    {
        this.progressBarTemplate = progressBarTemplate;
        this.achievementSO = achievementSO;
        root = CreateVisualElementFromTemplate();

        startVal = achievementSO.lockedInProgress;
        root.name = achievementSO.name;
        Label goalTitle = root.Q<Label>("goalTitle");
        goalTitle.text = achievementSO.achievementName + ":";
        goalCurrentValue = root.Q<Label>("goalCurrentValue");
        goalCurrentValue.text = achievementSO.lockedInProgress.ToString();
        progressBar = root.Q<ProgressBar>();
        progressBar.value = startVal / (float)achievementSO.target * 100f;
    }

    private VisualElement CreateVisualElementFromTemplate()
    {
        return progressBarTemplate.CloneTree();
    }
    
    public void AnimateProgressBar(int newVal)
    {
        // Create a temporary GameObject for tweening
        GameObject tweenGO = new GameObject("ProgressBarTween");
        float startValue = startVal;
        float endValue = newVal;
        float duration = 1f; // Duration of the tween in seconds

        

        // Start the tween
        LeanTween.value(tweenGO, startValue, endValue, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate(val =>
            {
                progressBar.value = val / achievementSO.target * 100f;
                goalCurrentValue.text = Mathf.FloorToInt(val).ToString();
            })
            .setOnComplete(() =>
            {
                progressBar.value = endValue / achievementSO.target * 100f;;
                goalCurrentValue.text = Mathf.FloorToInt(endValue).ToString();
                Destroy(tweenGO);
            });

        // Update startVal for future animations
        startVal = newVal;
    }
}