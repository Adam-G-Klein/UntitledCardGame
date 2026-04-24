using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum ShopDemoEvent
{
    None,
    EnterShop,
    CardOfferingTips,
}

[Serializable]
public class ShopDemoDialogueForEvent
{
    [SerializeField]
    public ShopDemoEvent shopDemoEvent;
    [SerializeField]
    [TextArea]
    public List<string> dialogueLines;

    public ShopDemoDialogueForEvent() {
        shopDemoEvent = ShopDemoEvent.None;
        dialogueLines = new List<string>();
    }
}

[Serializable]
public class ShopDemoDialogues
{
    [SerializeField]
    public List<ShopDemoDialogueForEvent> shopDemoDialoguesForEvents;

    public ShopDemoDialogues() {
        shopDemoDialoguesForEvents = new List<ShopDemoDialogueForEvent>();
    }
}

public class DemoDirector : GenericSingleton<DemoDirector>
{
    public DemoDataSO demoDataSO;
    public GameStateVariableSO gameState;
    public Sprite speakerSprite;

    public bool IsCombatOnboardingActive { get; private set; }
    [TextArea]
    public List<string> bendingTheRulesStepDialogue;
    [TextArea]
    public List<string> moreRewardsThanUsualStepDialogue;
    [TextArea]
    public List<string> startOfFirstStaticChooseNShopDialogue;
    [TextArea]
    public List<string> startOfSecondStaticChooseNShopDialogue;
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
    [TextArea]
    public List<string> prematureEndTurnReminderDialogueLines;
    [TextArea]
    public List<string> youShouldSpendYourMoneyBozoDialogueLines;

    // Shop demo dialogues for a specific shop (index 0 is the first shop, index 1 is the second shop, etc.)
    [SerializeField]
    public List<ShopDemoDialogues> shopDemoDialogues;

    public void Reset() {
        demoDataSO.stepCompletion = new Dictionary<DemoStepName, bool>();
        foreach (DemoStepName name in Enum.GetValues(typeof(DemoStepName))) {
            demoDataSO.stepCompletion[name] = false;
        }
    }

    public IEnumerator InvokeDemoStepForShopDemoEventCoroutine(ShopDemoEvent shopDemoEvent, int shopIndex) {
        if (shopIndex >= shopDemoDialogues.Count) {
            yield break;
        }

        ShopDemoDialogues dialoguesForShop = shopDemoDialogues[shopIndex];
        ShopDemoDialogueForEvent dialogueForEvent = dialoguesForShop.shopDemoDialoguesForEvents.Find(dialogue => dialogue.shopDemoEvent == shopDemoEvent);
        if (dialogueForEvent == null) {
            yield break;
        }

        foreach (string line in dialogueForEvent.dialogueLines) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
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

            case DemoStepName.StartOfFirstStaticChooseNShop:
                yield return StartOfFirstStaticChooseNShopStep();
            break;

            case DemoStepName.StartOfSecondStaticChooseNShop:
                yield return StartOfSecondStaticChooseNShopStep();
            break;

            case DemoStepName.BuyCompanionReminder:
                yield return BuyCompanionReminderStep();
            break;

            case DemoStepName.CardOfferingTips:
                yield return CardOfferingTipsStep();
            break;

            case DemoStepName.FullFeatureShopTutorialStep1:
                yield return FullFeatureShopTutorialStep1();
            break;

            case DemoStepName.FullFeatureShopTutorialStep2:
                yield return FullFeatureShopTutorialStep2();
            break;

            case DemoStepName.FullFeatureShopTutorialStep3:
                yield return FullFeatureShopTutorialStep3();
            break;

            case DemoStepName.FullFeatureShopTutorialStep4:
                yield return FullFeatureShopTutorialStep4();
            break;

            case DemoStepName.PostCombatRewardsDialogue:
                yield return PostCombatRewardsDialogueStep();
            break;

            case DemoStepName.PrematureEndTurnReminder:
                foreach (string line in prematureEndTurnReminderDialogueLines) {
                    yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
                }
                DialogueView.Instance.Hide();
                demoDataSO.stepCompletion[DemoStepName.PrematureEndTurnReminder] = true;
            break;

            case DemoStepName.YouShouldSpendYourMoneyBozo:
                foreach (string line in youShouldSpendYourMoneyBozoDialogueLines) {
                    yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
                }
                DialogueView.Instance.Hide();
                demoDataSO.stepCompletion[DemoStepName.YouShouldSpendYourMoneyBozo] = true;
                break;

            case DemoStepName.CombatTutorialStep:
                yield return CombatTutorialStep();
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

    public IEnumerator StartOfFirstStaticChooseNShopStep() {
        foreach (string line in startOfFirstStaticChooseNShopDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.StartOfFirstStaticChooseNShop] = true;
    }

    public IEnumerator StartOfSecondStaticChooseNShopStep() {
        foreach (string line in startOfSecondStaticChooseNShopDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.StartOfSecondStaticChooseNShop] = true;
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

    public IEnumerator FullFeatureShopTutorialStep1() {
        foreach (string line in secondShopTutorialStep1Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.FullFeatureShopTutorialStep1] = true;
    }

    public IEnumerator FullFeatureShopTutorialStep2() {
        foreach (string line in secondShopTutorialStep2Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.FullFeatureShopTutorialStep2] = true;
    }

    public IEnumerator FullFeatureShopTutorialStep3() {
        foreach (string line in secondShopTutorialStep3Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.FullFeatureShopTutorialStep3] = true;
    }

    public IEnumerator FullFeatureShopTutorialStep4() {
        foreach (string line in secondShopTutorialStep4Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.FullFeatureShopTutorialStep4] = true;
    }

    public IEnumerator PostCombatRewardsDialogueStep() {
        foreach (string line in postCombatRewardsDialogueDialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.PostCombatRewardsDialogue] = true;
    }

    public IEnumerator CombatTutorialStep() {
        IsCombatOnboardingActive = true;
        yield return CombatOnboardingDirector.Instance.RunAllStepsCoroutine();
        IsCombatOnboardingActive = false;
        gameState.HasSeenCombatTutorial = true;
        demoDataSO.stepCompletion[DemoStepName.CombatTutorialStep] = true;
    }
}

public enum DemoStepName {
    BendingTheRules,
    MoreRewardsThanUsual,
    StartOfFirstStaticChooseNShop,
    BuyCompanionReminder,
    CardOfferingTips,
    FullFeatureShopTutorialStep1,
    FullFeatureShopTutorialStep2,
    FullFeatureShopTutorialStep3,
    FullFeatureShopTutorialStep4,
    PostCombatRewardsDialogue,
    StartOfSecondStaticChooseNShop,
    PrematureEndTurnReminder,
    CombatTutorialStep,
    YouShouldSpendYourMoneyBozo,
}