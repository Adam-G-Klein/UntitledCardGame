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
    private MeothraOutroAnimationDisplay meothraOutroAnimationDisplay;
    [SerializeField] private GameObjectFocusable focusable;

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
        if (meothraOutroAnimationDisplay == null) meothraOutroAnimationDisplay = GetComponentInChildren<MeothraOutroAnimationDisplay>();
        if (focusable == null) focusable = GetComponent<GameObjectFocusable>();
        MusicController.Instance.PlayBossMusic();
        

        meothraIntentDisplay.Setup();
        meothraIntroAnimation.Setup();
        meothraAnimationController.Setup();
        meothraIntroAnimation.cinematicIntroCompleteHandler += () => {
            suppressSelectedIndicator = false;
            FocusManager.Instance.EnableFocusableTarget(focusable);
        };
        meothraOutroAnimationDisplay.Setup();
        enemyInstance.preEnactIntentHook += Attack;
        enemyInstance.combatInstance.onDamageHandler += OnDamageHandler;
        enemyInstance.combatInstance.onDeathHandler += OnDeath;
        FocusManager.Instance.DisableFocusableTarget(focusable);
    }

    private IEnumerator OnDeath(CombatInstance killer)
    {
        Debug.Log("MeothraController: OnDeath called!");
        FocusManager.Instance.UnregisterFocusableTarget(focusable);
        yield return StartCoroutine(meothraOutroAnimationDisplay.PlayOutroAnimation());
    }

    private IEnumerator Attack(List<Vector3> positions)
    {
        if(positions.Count == 1)
        {
            yield return StartCoroutine(
                meothraAnimationController.StrikeAnimation(positions[0])
            );
        } else
        {
            yield return StartCoroutine(meothraAnimationController.FullTeamStrikeAnimation(positions));
        }
        yield return null;
    }

    private void OnDamageHandler(int scale) {
        StartCoroutine(OnDamageVFX());
    }

    private IEnumerator OnDamageVFX() {
        meothraIntentDisplay.HideIntent();
        meothraAnimationController.PlayHurtAnimation();
        ScreenShakeManager.Instance.ShakeWithForce(1f);
        MusicController.Instance.PlaySFX("event:/SFX/BossFight/SFX_MeothraTakeDamage");
        yield return new WaitForSeconds(0.25f);
        meothraAnimationController.PlayIdleAnimation();
        meothraIntentDisplay.ShowIntent();
    }

    public void ShowHealthBar()
    {
        if(!meothraHealthDisplay.isSetup)
        {
            meothraHealthDisplay.Setup();
        }
        meothraHealthDisplay.ShowView();

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