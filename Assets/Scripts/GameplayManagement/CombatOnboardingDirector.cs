using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[Serializable]
public enum CombatOnboardingStepName
{
    Lines0To1,
}
/*
Runs through this script: https://docs.google.com/document/d/1r_jzg6FcQWvHmvWbQ557McmXIgseUmjPlC_q-Y6Xk2Y/edit?tab=t.9okxt9anqbnd
*/

public class CombatOnboardingDirector : GenericSingleton<CombatOnboardingDirector>
{
    public Sprite speakerSprite;
    // public string lineFor0To1Beat = "...";

    [Header("Refer to this sheet for line id associations: https://docs.google.com/spreadsheets/d/1sCZju8l5PAlu4M5Gi5D9lO3MF4LxUBD1K0KaEl6SMws/edit?usp=sharing")]
    [TextArea]
    public List<string> dialogueLines;
    public InputActionReference continueInput;
    public SpriteRenderer baronShadow;
    public Vector3 visibleShadowPosition;
    public Vector3 hiddenShadowPosition;
    public AnimationCurve companionKickInCurve;
    public AnimationCurve waddleCurve;

    [Header("Intro Camera Tour")]
    public Transform cameraTourTargetsParent;
    public float cameraDollyDuration = 2.5f;
    public float cameraTourHoldInBlack = 0.35f;
    public float cameraFadeDuration = 0.5f;
    [SerializeField] private float cameraTourZoom = 5f;

    private EnemyEncounterManager manager;
    private Cinemachine.CinemachineVirtualCamera virtualCamera;
    private int dialogueLineIndex;
    private bool pause = false;

    public IEnumerator RunAllStepsCoroutine()
    {
        manager = EnemyEncounterManager.Instance;
        virtualCamera = ScreenShakeManager.Instance.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        dialogueLineIndex = 0;
        // calls speakline for the first time. Just needs the dialogue to be in screenspace mode, and does the setup for that
        yield return CameraTourCoroutine();
        yield return new WaitForSeconds(0.5f); // brief delay after camera tour
        yield return SpeakLineNoHide();
        manager.combatEncounterView.SetupFromGamestate(manager, true);
        FocusManager.Instance.StashFocusables(this.GetType().Name);
        pause = true;
        AnimateCompanionsFromOffscreen();
        yield return new WaitUntil(() => pause == false); // pause gets set to false in AnimateCompanionsFromOffscreen
        // Enemy encounter manager will hang during PreEncounterCoroutine and wait for this
        // large coroutine to finish
        yield return manager.StartWhenUIDocReady();
        FocusManager.Instance.StashFocusables(this.GetType().Name);
        CombatEntityManager.Instance.getCompanions().ForEach((companion) => companion.SetBackgroundGradientVisible(false));
        CombatEntityManager.Instance.getEnemies().ForEach((enemy) => enemy.SetBackgroundGradientVisible(false));
        manager.combatEncounterView.ShowOnlyCompanionSprites();
        yield return SpeakLine();
        yield return ShowCompanionTooltips();
        continueInput.action.Enable();
        yield return new WaitForNextFrameUnit(); // Need to wait a frame or it'll pickup the click from continuing dialogue
        manager.combatEncounterView.ToggleContinuePromptVisibility(true);
        yield return new WaitUntil(() => continueInput.action.WasPerformedThisFrame());
        manager.combatEncounterView.ToggleContinuePromptVisibility(false);
        CombatEntityManager.Instance.getCompanions().ForEach((companion) => ShowHideTooltip(companion.gameObject, false));
        yield return SpeakLine();
        foreach (CompanionInstance companion in CombatEntityManager.Instance.getCompanions()) {
            yield return manager.combatEncounterView.ShowCardsFromCompanion(companion);
            // show combatUI click to proceed 
            manager.combatEncounterView.ToggleContinuePromptVisibility(true);
            yield return new WaitForNextFrameUnit();
            yield return new WaitUntil(() => continueInput.action.WasPerformedThisFrame());
            manager.combatEncounterView.ToggleContinuePromptVisibility(false);
            LeanTween.cancelAll();
            pause = true;
            manager.combatEncounterView.HideCardsAndShowCompanionFrame(companion, () => pause = false);
            yield return new WaitUntil(() => pause == false);
        }
        // Change to combat music
        AnimateEnemiesFromOffscreen();
        pause = true;
        yield return SpeakLine();
        yield return new WaitUntil(() => pause == false);
        yield return SpeakLine();
        MusicController.Instance.PlaySFX("event:/SFX/SFX_PlayerEndTurn");
        ScreenShakeManager.Instance.ShakeWithForce(0.1f);
        yield return new WaitForSeconds(.1f);
        ScreenShakeManager.Instance.ShakeWithForce(0.1f);
        MusicController.Instance.PlaySFX("event:/SFX/SFX_PlayerEndTurn");
        yield return new WaitForSeconds(1f);
        yield return SpeakLineNoHide();
        yield return SpeakLine();
        CombatEntityManager.Instance.getCompanions().ForEach((companion) => {
            companion.deckInstance.MoveCardToTopOfDrawPile("Scratch");
            companion.deckInstance.DealOneCard();
        });
        yield return new WaitUntil(() => PlayerHand.Instance.CardDealQueue.Count == 0);
        PlayerHand.Instance.DisableHand();
        yield return new WaitForSeconds(2f);
        manager.combatEncounterView.SetManaIndicatorVisible();
        yield return SpeakLine();
        yield return SpeakLine();

        // Wait for the player to play a card
        PlayerHand.Instance.EnableHand();
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        manager.combatEncounterView.SetCompanionsAndEnemiesEnabled(true);
        pause = true;
        PlayerHand.Instance.onAttackCardCastDispatcher.AddHandler(ContinueAfterCardPlayed, 0);
        yield return new WaitUntil(() => pause == false);
        yield return new WaitForNextFrameUnit();
        PlayerHand.Instance.onAttackCardCastDispatcher.RemoveHandler(ContinueAfterCardPlayed);
        PlayerHand.Instance.DisableHand();
        FocusManager.Instance.StashFocusables(this.GetType().Name);

        // Concierge talking, baron shadow coming in, concierge being summoned away again
        pause = true;
        AnimateBaronShadowComingIn();
        yield return SpeakLine();
        yield return new WaitUntil(() => pause == false);
        MusicController.Instance.PlaySFX("event:/SFX/SFX_PlayerEndTurn");
        ScreenShakeManager.Instance.ShakeWithForce(0.5f);
        yield return new WaitForSeconds(.1f);
        ScreenShakeManager.Instance.ShakeWithForce(0.5f);
        MusicController.Instance.PlaySFX("event:/SFX/SFX_PlayerEndTurn");
        yield return new WaitForSeconds(.1f);
        ScreenShakeManager.Instance.ShakeWithForce(0.5f);
        MusicController.Instance.PlaySFX("event:/SFX/SFX_PlayerEndTurn");
        yield return new WaitForSeconds(1f);

        // Concierge leaving you, baron shadow moving back away
        yield return SpeakLineNoHide();
        StartCoroutine(AnimateBaronShadowGoingOut());
        yield return SpeakLine();

        // Finish remaining setup for the rest of the combat
        manager.combatEncounterView.SetPersistentElementsVisible(true);
        PlayerHand.Instance.EnableHand();
        FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        List<PlayableCard> cardsInHand = new List<PlayableCard>(PlayerHand.Instance.GetCardsOrdered());
        cardsInHand.ForEach((card) => StartCoroutine(PlayerHand.Instance.DiscardCard(card)));
        manager.combatEncounterView.CompanionViews.ForEach((view) => view.EnableInteractions());
    }

    private IEnumerator SpeakLine()
    {
        yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, dialogueLines[dialogueLineIndex], true);
        DialogueView.Instance.Hide();
        dialogueLineIndex += 1;
    }

    private IEnumerator SpeakLineNoHideNoWait()
    {
        StartCoroutine(DialogueView.Instance.SpeakLineCoroutine(speakerSprite, dialogueLines[dialogueLineIndex], false));
        dialogueLineIndex += 1;
        yield break;
    }

    private IEnumerator SpeakLineNoHideNoWaitDelayed(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        yield return SpeakLineNoHideNoWait();
    }

    private IEnumerator SpeakLineNoHide()
    {
        yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, dialogueLines[dialogueLineIndex], true);
        dialogueLineIndex += 1;
    }

    private IEnumerator ContinueAfterCardPlayed(PlayableCard card) {
        pause = false;
        yield break;
    }

    private void ShowHideTooltip(GameObject companion, bool visible) {
        if (companion.TryGetComponent<TooltipOnHover>(out var tooltip)) {
            if (visible) {
                tooltip.DisplayTooltip();
                MusicController.Instance.PlaySFX("event:/SFX/SFX_UIHover");
            }
            else tooltip.OnPointerExitVoid();
        }
    }

    private void AnimateCompanionsFromOffscreen() {
        long delay = 100;
        foreach (CompanionView comp in manager.combatEncounterView.CompanionViews) {
            comp.SetEverythingHidden();
            comp.DisableInteractions();
            AnimateCompanionFromOffscreen(comp, 2f, delay);
            delay += 100;
        }
    }

    private void AnimateCompanionFromOffscreen(CompanionView comp, float seconds, long delay) {
        comp.container.schedule.Execute(() => {
            comp.SetOnlySpriteVisible();
            float startX = -(comp.container.worldBound.xMin + comp.container.worldBound.width + 20f);
            comp.container.style.translate = new Translate(startX, 0);
            LeanTween.value(0, 1, seconds)
                .setOnUpdate((float val) => {
                    float t = companionKickInCurve.Evaluate(val);
                    float x = Mathf.Lerp(startX, 0, t);
                    comp.container.style.translate = new Translate(x, 0);
                })
                .setOnComplete(() => pause = false);
            LeanTween.value(0f, 5f, seconds)
                .setOnUpdate((float val) => {
                    float t = waddleCurve.Evaluate(Mathf.Repeat(val, 1f));
                    comp.container.style.rotate = new Rotate(new Angle(t * 10f, AngleUnit.Degree));
                })
                .setOnComplete(() => {
                    LeanTween.value(comp.container.style.rotate.value.angle.value, 0, 0.2f)
                        .setOnUpdate((float val) => {
                            comp.container.style.rotate = new Rotate(new Angle(val, AngleUnit.Degree));
                        });
                });
        }).ExecuteLater(delay);
    }

    private void AnimateEnemiesFromOffscreen() {
        long delay = 100;
        foreach (EnemyView enemy in manager.combatEncounterView.EnemyViews) {
            enemy.SetEverythingHidden();
            enemy.DisableInteractions();
            AnimateEnemiesFromOffscreen(enemy, 2f, delay);
            delay += 100;
        }
    }

    private void AnimateEnemiesFromOffscreen(EnemyView enemy, float seconds, long delay) {
        enemy.container.schedule.Execute(() => {
            enemy.SetOnlySpriteVisible();
            float startY = -(enemy.container.worldBound.yMin + enemy.container.worldBound.height + 20f);
            enemy.container.style.translate = new Translate(0, startY);
            LeanTween.value(startY, 0, seconds)
                .setEase(LeanTweenType.easeOutCubic)
                .setOnUpdate((float val) => {
                    enemy.container.style.translate = new Translate(0, val);
                })
                .setOnComplete(() => {
                    pause = false;
                    CombatEntityManager.Instance.getEnemies().ForEach((enemy) => manager.combatEncounterView.ShowEnemyFrame(enemy));
                });
            LeanTween.value(0f, seconds / 1.5f, seconds / 1.5f)
                .setOnUpdate((float t) => {
                    float normalized = (t / seconds / 1.5f) * 4;
                    float angle = Mathf.Sin(normalized * Mathf.PI * 2f) * 10;
                    enemy.container.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
                })
                .setOnComplete(() => {
                    LeanTween.value(enemy.container.style.rotate.value.angle.value, 0, 0.2f)
                        .setOnUpdate((float val) => {
                            enemy.container.style.rotate = new Rotate(new Angle(val, AngleUnit.Degree));
                        });
                });
        }).ExecuteLater(delay);
    }

    private void AnimateBaronShadowComingIn() {
        float duration = 2f;
        baronShadow.enabled = true;
        LeanTween.move(baronShadow.gameObject, visibleShadowPosition, duration)
            .setOnComplete(() => pause = false);
    }
    private IEnumerator CameraTourCoroutine() {
        if (virtualCamera == null || cameraTourTargetsParent == null || cameraTourTargetsParent.childCount < 2) {
            yield break;
        }

        DialogueView.Instance.SetScreenSpaceMode(true);
        StartCoroutine(SpeakLineNoHideNoWaitDelayed(0.5f));
        Vector3 startPosition = virtualCamera.transform.position;
        float startOrthographicSize = virtualCamera.m_Lens.OrthographicSize;
        int childCount = cameraTourTargetsParent.childCount;
        try {
            SceneTransitionManager.Instance.SetSortingOrder(100);
            yield return SceneTransitionManager.Instance.Fade(1f);
            yield return new WaitForSeconds(cameraTourHoldInBlack);
            virtualCamera.m_Lens.OrthographicSize = cameraTourZoom;

            for (int i = 0; i + 1 < childCount; i += 2) {
                Transform snapTarget = cameraTourTargetsParent.GetChild(i);
                Transform panTarget = cameraTourTargetsParent.GetChild(i + 1);

                virtualCamera.transform.position = new Vector3(snapTarget.position.x, snapTarget.position.y, virtualCamera.transform.position.z);

                // Start pan immediately, then fade in — camera is already moving when visible
                Vector3 flatTarget = new Vector3(panTarget.position.x, panTarget.position.y, virtualCamera.transform.position.z);
                float panStartTime = Time.time;
                LeanTween.move(virtualCamera.gameObject, flatTarget, cameraDollyDuration);

                yield return SceneTransitionManager.Instance.Fade(0f);

                // Wait until cameraFadeDuration before pan ends, then fade out while camera is still moving
                float elapsed = Time.time - panStartTime;
                float waitTime = cameraDollyDuration - elapsed - cameraFadeDuration;
                if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

                if(i == childCount - 1) {
                    DialogueView.Instance.SetScreenSpaceSortingOrder("Overlay UI", 99);
                }
                yield return SceneTransitionManager.Instance.Fade(1f);
                LeanTween.cancel(virtualCamera.gameObject);
                yield return new WaitForSeconds(cameraTourHoldInBlack);
            }

            virtualCamera.transform.position = startPosition;
            virtualCamera.m_Lens.OrthographicSize = startOrthographicSize;
        } finally {
            DialogueView.Instance.Hide();
            DialogueView.Instance.SetScreenSpaceMode(false);
            StartCoroutine(SceneTransitionManager.Instance.Fade(0f));
        }
        yield return new WaitForSeconds(SceneTransitionManager.Instance.fadeTime);
        SceneTransitionManager.Instance.ResetSortingOrder();
    }

    private IEnumerator DollyCameraTo(Vector3 target, float duration) {
        bool done = false;
        LeanTween.move(virtualCamera.gameObject, target, duration)
            .setOnComplete(() => done = true);
        yield return new WaitUntil(() => done);
    }

    private IEnumerator AnimateBaronShadowGoingOut() {
        float duration = 2f;
        bool done = false;
        LeanTween.move(baronShadow.gameObject, hiddenShadowPosition, duration)
            .setOnComplete(() => done = true);
        yield return new WaitUntil(() => done == true);
        baronShadow.enabled = false;
    }

    private IEnumerator ShowCompanionTooltips()
    {
        float delay = .5f;
        for (int i = 0; i < CombatEntityManager.Instance.getCompanions().Count; i++) {
            CompanionInstance companion = CombatEntityManager.Instance.getCompanions()[i];
            ShowHideTooltip(companion.gameObject, true);
            yield return new WaitForSeconds(delay);        
        };
        // this isn't strictly necessary, but makes it so that the tooltips finish rendering before
        // the click to proceed button shows up
        yield return new WaitForSeconds(delay);
    }
}
