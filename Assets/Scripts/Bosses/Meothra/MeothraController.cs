using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MeothraController : MonoBehaviour, IBossController
{
    [SerializeField] public EnemyInstance enemyInstance;
    [SerializeField] private MeothraIntentDisplay meothraIntentDisplay;
    [SerializeField] private MeothraIntroAnimationDisplay meothraIntroAnimation;
    [SerializeField] private MeothraHealthDisplay meothraHealthDisplay;

    public void Setup()
    {
        if (enemyInstance == null) enemyInstance = GetComponent<EnemyInstance>();
        if (meothraIntentDisplay == null) meothraIntentDisplay = GetComponent<MeothraIntentDisplay>();
        if (meothraIntroAnimation == null) meothraIntroAnimation = GetComponentInChildren<MeothraIntroAnimationDisplay>();
        if (meothraHealthDisplay == null) meothraHealthDisplay = GetComponentInChildren<MeothraHealthDisplay>();

        meothraIntentDisplay.Setup();
        meothraIntroAnimation.Setup();
        meothraIntroAnimation.cinematicIntroCompleteHandler += meothraHealthDisplay.ShowView;
        enemyInstance.preEnactIntentHook += Attack;
        enemyInstance.combatInstance.onDamageHandler += OnDamageHandler;
    }

    private IEnumerator Attack(List<Vector3> positions)
    {
        yield return null;
    }

    private void OnDamageHandler(int scale) {
        StartCoroutine(OnDamageVFX());
    }

    private IEnumerator OnDamageVFX() {
        meothraIntentDisplay.HideIntent();
        ScreenShakeManager.Instance.ShakeWithForce(1f);
        yield return new WaitForSeconds(0.25f);
        meothraIntentDisplay.ShowIntent();
    }

    public Vector3 GetFrameLocation()
    {
        return enemyInstance.placement.worldPos;
    }

}
