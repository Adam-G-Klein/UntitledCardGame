using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class MeothraOutroAnimationDisplay : MonoBehaviour
{

    private Animator animator;
    private PlayableDirector director;
    [SerializeField] private TimelineAsset outroTimeline;
    private MeothraIntroAnimationDisplay meothraIntroAnimationDisplay;
    private MeothraVFXController meothraVFXController;

    public void Setup()
    {
        director = GetComponent<PlayableDirector>();
        animator = GetComponentInChildren<Animator>();
        meothraIntroAnimationDisplay = GetComponent<MeothraIntroAnimationDisplay>();
        meothraVFXController = GetComponentInChildren<MeothraVFXController>();
        if (animator == null)
        {
            Debug.LogError("MeothraOutroAnimationDisplay: Setup could not find Animator!");
            return;
        }

        if(director == null)
        {
            Debug.LogError("MeothraOutroAnimationDisplay: Setup could not find PlayableDirector!");
            return;
        }

        if (meothraVFXController == null)
        {
            Debug.LogError("MeothraOutroAnimationDisplay: Setup could not find MeothraVFXController!");
            return;
        }
    }
    public IEnumerator PlayOutroAnimation()
    {
        meothraIntroAnimationDisplay.currentShakeIndex = 2;
        director.playableAsset = outroTimeline;
        director.time = 0.0f;
        director.Play();
        yield return new WaitForSeconds((float)outroTimeline.duration);
        Debug.Log("MeothraOutroAnimationDisplay: Outro animation complete!");
        EnemyEncounterManager.Instance.CinematicOutroComplete();
    }

    public void TweenOutIdleMaterials()
    {
        meothraVFXController.TweenOutIdleMaterials();
    }

    public void RecreateMapAndEnemyUI()
    {
        EnemyEncounterManager.Instance.combatEncounterView.RecreateMapAndEnemyUI();
    }
}