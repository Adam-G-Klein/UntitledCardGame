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
    private MeothraAnimationController meothraAnimationController;

    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private GameObject specialAttackVFX;

    private bool suppressSelectedIndicator = true;

    public void Setup()
    {
        if (enemyInstance == null) enemyInstance = GetComponent<EnemyInstance>();
        if (meothraIntentDisplay == null) meothraIntentDisplay = GetComponent<MeothraIntentDisplay>();
        if (meothraIntroAnimation == null) meothraIntroAnimation = GetComponentInChildren<MeothraIntroAnimationDisplay>();
        if (meothraHealthDisplay == null) meothraHealthDisplay = GetComponentInChildren<MeothraHealthDisplay>();
        if (meothraAnimationController == null) meothraAnimationController = GetComponentInChildren<MeothraAnimationController>();

        meothraIntentDisplay.Setup();
        meothraIntroAnimation.Setup();
        meothraAnimationController.Setup();
        meothraIntroAnimation.cinematicIntroCompleteHandler += () => suppressSelectedIndicator = false;
        enemyInstance.preEnactIntentHook += Attack;
        enemyInstance.combatInstance.onDamageHandler += OnDamageHandler;
    }

    private IEnumerator Attack(List<Vector3> positions)
    {
        MusicController.Instance.PlaySFX("event:/SFX/BossFight/SFX_MeothraAttack");
        yield return StartCoroutine(
            meothraAnimationController.StrikeAnimation(positions[0])
        );
        yield return null;
    }

    private void OnDamageHandler(int scale) {
        StartCoroutine(OnDamageVFX());
    }

    private IEnumerator OnDamageVFX() {
        meothraIntentDisplay.HideIntent();
        ScreenShakeManager.Instance.ShakeWithForce(1f);
        MusicController.Instance.PlaySFX("event:/SFX/BossFight/SFX_MeothraTakeDamage");
        yield return new WaitForSeconds(0.25f);
        meothraIntentDisplay.ShowIntent();
    }

    public Vector3 GetFrameLocation()
    {
        return enemyInstance.placement.worldPos;
    }

    public void EnableSelectedIndicator() {
        if (suppressSelectedIndicator) return; // Wait for cinematic intro to be over
        selectedIndicator.SetActive(true);
    }

    public void DisableSelectedIndicator() {
        selectedIndicator.SetActive(false);
    }
}
