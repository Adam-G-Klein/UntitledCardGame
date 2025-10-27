using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MeothraController : MonoBehaviour, IBossController
{
    [SerializeField] private EnemyInstance enemyInstance;
    [SerializeField] private MeothraIntentDisplay meothraIntentDisplay;
    [SerializeField] private MeothraIntroAnimationDisplay meothraIntroAnimation;

    public void Setup()
    {
        if (enemyInstance == null) enemyInstance = GetComponent<EnemyInstance>();
        if (meothraIntentDisplay == null) meothraIntentDisplay = GetComponent<MeothraIntentDisplay>();
        if (meothraIntroAnimation == null) meothraIntroAnimation = GetComponentInChildren<MeothraIntroAnimationDisplay>();

        meothraIntentDisplay.Setup();
        meothraIntroAnimation.Setup();
        enemyInstance.preEnactIntentHook += Attack;
    }

    private IEnumerator Attack(List<Vector3> positions) {
        yield return null;
    }
}
