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
    [Header("Shake Magnitude, Duration, pingpongs")]
    public List<Vector3> rotationShakeScales = new List<Vector3>() { new Vector3(1, 1, 3), new Vector3(2, 2, 4), new Vector3(3, 3, 5)};
    public bool rotationShake = false;
    public bool positionShake = false;
    [Header("Shake Magnitude, Duration (not used, just half rotation duration), pingpongs")]
    public List<float> positionShakes = new List<float>() { 10f, 20f, 30f };
    
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
        Vector3 currRotationShake = rotationShakeScales[currentShakeIndex];
        float duration = currRotationShake.y;

        // LOOK IT WORKS
        // have to grab the second one because that's the one that's set up after the cirular dependency is resolved
        EnemyView view = EnemyEncounterManager.Instance.combatEncounterView.GetEnemyViews()[1];
        if(rotationShake)
            view.BossFrameDestructionRotationShake(currRotationShake.x, duration, (int) currRotationShake.z);
        if(positionShake)
        {
            float currentPositionMagnitude = positionShakes[currentShakeIndex];
            view.BossFrameDestructionPositionShake(currentPositionMagnitude, duration , (int)currRotationShake.z);
            // schedule another one, inverted scale, halfway through the rotation shake
            /*view.BossFrameDestructionPositionShake( - currentPositionMagnitude,
                duration/2f,
                (int)Math.Floor(currRotationShake.z/2),
                currRotationShake.y / 2f // delay by half the duration of the rotation shake
                );
                */

        }

        currentShakeIndex++;
    }

    public void CinematicIntroComplete()
    {
        EnemyEncounterManager.Instance.CinematicIntroComplete();
    }

}
