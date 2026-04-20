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

    private EnemyEncounterManager manager;
    private int dialogueLineIndex;
    private bool pause = false;

    public IEnumerator RunAllStepsCoroutine()
    {
        manager = EnemyEncounterManager.Instance;
        dialogueLineIndex = 0;
        yield return SpeakLineNoHide();
        yield return SpeakLineNoHide();
        yield return SpeakLine();
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
        CombatEntityManager.Instance.getCompanions().ForEach((companion) => ShowHideTooltip(companion.gameObject, true));
        continueInput.action.Enable();
        yield return new WaitForNextFrameUnit(); // Need to wait a frame or it'll pickup the click from continuing dialogue
        yield return new WaitUntil(() => continueInput.action.WasPerformedThisFrame());
        CombatEntityManager.Instance.getCompanions().ForEach((companion) => ShowHideTooltip(companion.gameObject, false));
        yield return SpeakLine();
        foreach (CompanionInstance companion in CombatEntityManager.Instance.getCompanions()) {
            manager.combatEncounterView.ShowCardsFromCompanion(companion);
            yield return new WaitForNextFrameUnit();
            yield return new WaitUntil(() => continueInput.action.WasPerformedThisFrame());
            pause = true;
            manager.combatEncounterView.HideCardsAndShowCompanionFrame(companion, () => pause = false);
            yield return new WaitUntil(() => pause == false);
        }
        // Change to combat music
        yield return SpeakLine();
        // Enemies saunter up, and pedestals form
        AnimateEnemiesFromOffscreen();
        pause = true;
        yield return new WaitUntil(() => pause == false);
        yield return SpeakLine();
        // Bell to summon concierge away SFX and screen shake
        yield return SpeakLineNoHide();
        yield return SpeakLine();
        // Set all decks to have strikes on top and draw cards
        yield return SpeakLine();
        // Mana indicator appears
        yield return SpeakLine();
        // Wait until the player plays a slash
        yield return SpeakLine();
        // Baron shadow
        yield return SpeakLine();
        // louder bell and bigger screen shake
        yield return SpeakLineNoHide();
        yield return SpeakLine();

    }

    private IEnumerator SpeakLine()
    {
        yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, dialogueLines[dialogueLineIndex], true);
        DialogueView.Instance.Hide();
        dialogueLineIndex += 1;
    }

    private IEnumerator SpeakLineNoHide()
    {
        yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, dialogueLines[dialogueLineIndex], true);
        dialogueLineIndex += 1;
    }

    private void ShowHideTooltip(GameObject companion, bool visible) {
        if (companion.TryGetComponent<TooltipOnHover>(out var tooltip)) {
            if (visible) tooltip.OnPointerEnterVoid();
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
            LeanTween.value(startX, 0, seconds)
                .setEase(LeanTweenType.easeOutCubic)
                .setOnUpdate((float val) => {
                    comp.container.style.translate = new Translate(val, 0);
                })
                .setOnComplete(() => pause = false);
            LeanTween.value(0f, seconds / 1.5f, seconds / 1.5f)
                .setOnUpdate((float t) => {
                    float normalized = (t / seconds / 1.5f) * 4;
                    float angle = Mathf.Sin(normalized * Mathf.PI * 2f) * 10;
                    comp.container.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
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
}
