using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
        manager.combatEncounterView.AnimateCompanionsFromOffscreen();
        // Should make the companion animation yield return but couldn't be bothered rn
        yield return new WaitForSeconds(3f);
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
}
