using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class MeothraIntroAnimationDisplay : MonoBehaviour
{

    /*
        float duration = 0.125f;  // Total duration for the scale animation
        float minScale = .8f; // (float)Math.Min(.75, .9 - scale / 500);  // scale bump increases in intensity if entity takes more damage (haven't extensively tested this)
    */
    [Header("Shake Magnitude, Duration")]
    public List<Vector2> rotationShakeScales = new List<Vector2>() { new Vector2(1, 1), new Vector2(2, 2), new Vector2(3, 3)};
    [SerializeField]
    private int currentShakeIndex = 0;
    private PlayableDirector playableDirector;

    public void Setup()
    {
        /*
        if(startingFrameView.GetEntity().GetEnemyInstance().GetDisplayType() != DisplayType.MEOTHRA)
        {
            Debug.LogError("MeothraIntroAnimationDisplay: Setup called but the first enemy is not Meothra!");
            return;
        }
        */
        playableDirector = GetComponent<PlayableDirector>();
        playableDirector.Play();
    }
    public void DoNextShake()
    {
        Debug.Log("Meothra, called ui shake!");
        if (currentShakeIndex >= rotationShakeScales.Count)
        {
            Debug.Log("MeothraIntroAnimationDisplay: DoNextShake called but there are no more shakes to do!");
            return;
        }
        //Vector2 currRotationShake = rotationShakeScales[currentShakeIndex];
        //startingFrameView.BossFrameDestructionRotationShake(currRotationShake.x, currRotationShake.y);

        // LOOK IT WORKS
        EnemyView view = EnemyEncounterManager.Instance.combatEncounterView.GetEnemyViews()[1];
        view.DamageScaleBump(1);
        currentShakeIndex++;
    }

    public void CinematicIntroComplete()
    {
        EnemyEncounterManager.Instance.CinematicIntroComplete();
    }

}
