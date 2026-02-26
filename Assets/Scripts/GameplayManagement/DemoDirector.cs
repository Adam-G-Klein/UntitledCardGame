using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoDirector : GenericSingleton<DemoDirector>
{
    public DemoDataSO demoDataSO;
    public Sprite speakerSprite;
    [TextArea]
    public List<string> bendingTheRulesStepDialogue;
    [TextArea]
    public List<string> moreRewardsThanUsualStepDialogue;
    [TextArea]
    public List<string> startOfShopStepDialogue;
    [TextArea]
    public List<string> buyCompanionReminderStepDialogue;
    [TextArea]
    public List<string> cardOfferingTipsDialogue;
    [TextArea]
    public List<string> secondShopTutorialStep1Dialogue;
    [TextArea]
    public List<string> secondShopTutorialStep2Dialogue;
    [TextArea]
    public List<string> secondShopTutorialStep3Dialogue;
    [TextArea]
    public List<string> secondShopTutorialStep4Dialogue;
    [TextArea]
    public List<string> postCombatRewardsDialogueDialogue;

    public void Reset() {
        demoDataSO.stepCompletion = new Dictionary<DemoStepName, bool>();
        foreach (DemoStepName name in Enum.GetValues(typeof(DemoStepName))) {
            demoDataSO.stepCompletion[name] = false;
        }
    }

    public bool IsStepCompleted(DemoStepName stepName) {
        // Shouldn't hit this case but worth checking
        if(demoDataSO.stepCompletion == null) // necessary when running in editor
        {
            Reset();
        }
        if (!demoDataSO.stepCompletion.ContainsKey(stepName)) {
            return true;
        }

        return demoDataSO.stepCompletion[stepName];
    }

    public IEnumerator InvokeDemoStepCoroutine(DemoStepName demoStepName) {
        switch (demoStepName) {
            case DemoStepName.BendingTheRules:
                yield return BendingTheRulesStep();
            break;

            case DemoStepName.MoreRewardsThanUsual:
                yield return MoreRewardsThanUsualStep();
            break;

            case DemoStepName.StartOfShop:
                yield return StartOfShopStep();
            break;

            case DemoStepName.BuyCompanionReminder:
                yield return BuyCompanionReminderStep();
            break;

            case DemoStepName.CardOfferingTips:
                yield return CardOfferingTipsStep();
            break;

            case DemoStepName.SecondShopTutorialStep1:
                yield return SecondShopTutorialStep1();
            break;

            case DemoStepName.SecondShopTutorialStep2:
                yield return SecondShopTutorialStep2();
            break;

            case DemoStepName.SecondShopTutorialStep3:
                yield return SecondShopTutorialStep3();
            break;

            case DemoStepName.SecondShopTutorialStep4:
                yield return SecondShopTutorialStep4();
            break;

            case DemoStepName.PostCombatRewardsDialogue:
                yield return PostCombatRewardsDialogueStep();
            break;
        }
        yield return null;
    }

    public IEnumerator BendingTheRulesStep() {
        foreach (string line in bendingTheRulesStepDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.BendingTheRules] = true;
    }

    public IEnumerator MoreRewardsThanUsualStep() {
        foreach (string line in moreRewardsThanUsualStepDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.MoreRewardsThanUsual] = true;
    }

    public IEnumerator StartOfShopStep() {
        foreach (string line in startOfShopStepDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.StartOfShop] = true;
    }

    public IEnumerator BuyCompanionReminderStep() {
        foreach (string line in buyCompanionReminderStepDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.BuyCompanionReminder] = true;
    }

    public IEnumerator CardOfferingTipsStep() {
        foreach (string line in cardOfferingTipsDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.CardOfferingTips] = true;
    }

    public IEnumerator SecondShopTutorialStep1() {
        foreach (string line in secondShopTutorialStep1Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.SecondShopTutorialStep1] = true;
    }

    public IEnumerator SecondShopTutorialStep2() {
        foreach (string line in secondShopTutorialStep2Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.SecondShopTutorialStep2] = true;
    }

    public IEnumerator SecondShopTutorialStep3() {
        foreach (string line in secondShopTutorialStep3Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.SecondShopTutorialStep3] = true;
    }

    public IEnumerator SecondShopTutorialStep4() {
        foreach (string line in secondShopTutorialStep4Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.SecondShopTutorialStep4] = true;
    }

    public IEnumerator PostCombatRewardsDialogueStep() {
        foreach (string line in postCombatRewardsDialogueDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.PostCombatRewardsDialogue] = true;
    }
}

public enum DemoStepName {
    BendingTheRules,
    MoreRewardsThanUsual,
    StartOfShop,
    BuyCompanionReminder,
    CardOfferingTips,
    SecondShopTutorialStep1,
    SecondShopTutorialStep2,
    SecondShopTutorialStep3,
    SecondShopTutorialStep4,
    PostCombatRewardsDialogue
}