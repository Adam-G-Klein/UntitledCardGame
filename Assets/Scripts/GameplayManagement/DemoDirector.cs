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

    public void Reset() {
        demoDataSO.stepCompletion = new Dictionary<DemoStepName, bool>();
        foreach (DemoStepName name in Enum.GetValues(typeof(DemoStepName))) {
            demoDataSO.stepCompletion[name] = false;
        }
    }

    public bool IsStepCompleted(DemoStepName stepName) {
        // Shouldn't hit this case but worth checking
        if (!demoDataSO.stepCompletion.ContainsKey(stepName)) {
            return true;
        }

        return demoDataSO.stepCompletion[stepName];
    }

    public IEnumerator InvokeDemoStepCorouutine(DemoStepName demoStepName) {
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
}

public enum DemoStepName {
    BendingTheRules,
    MoreRewardsThanUsual,
    StartOfShop,
    BuyCompanionReminder
}