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

    public void Setup()
    {
        director = GetComponent<PlayableDirector>();
        animator = GetComponentInChildren<Animator>();
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
    }
    public IEnumerator PlayOutroAnimation()
    {
        director.playableAsset = outroTimeline;
        director.Play();
        yield return new WaitUntil(() => director.state != PlayState.Playing);
    }
}