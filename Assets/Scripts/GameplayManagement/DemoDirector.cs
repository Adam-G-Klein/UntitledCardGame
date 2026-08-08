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
    public List<string> cardBuyingTutorialStep1Dialogue;
    [TextArea]
    public List<string> cardBuyingTutorialStep2Dialogue;
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
    [TextArea]
    public List<string> boosterPacksShopTutorialStep1Dialogue;
    [TextArea]
    public List<string> boosterPacksShopTutorialStep2Dialogue;
    [TextArea]
    public List<string> boosterPacksShopTutorialStep3Dialogue;
    [TextArea]
    public List<string> boosterPacksShopTutorialStep4Dialogue;

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
        if(demoDataSO.stepCompletion == null) // necessary when running in editor
        {
            Reset();
        }
        if(stepName == DemoStepName.CombatTutorialStep
            && gameState.HasSeenCombatTutorial) {
            return true;
        }


        if (!demoDataSO.stepCompletion.ContainsKey(stepName)) {
            return true;
        }

        return demoDataSO.stepCompletion[stepName];
    }

    public IEnumerator InvokeDemoStepCoroutine(DemoStepName demoStepName) {
        // Special handling for combat tutorial step.
        if (demoStepName == DemoStepName.CombatTutorialStep) {
            yield return CombatTutorialStep();
            yield break;
        }
        // Special handling for card buying tutorial step.
        if (demoStepName == DemoStepName.CardBuyingTutorial) {
            yield return CardBuyingTutorial();
            yield break;
        }

        List<string> dialogueLines = new List<string>();
        switch (demoStepName) {
            case DemoStepName.BendingTheRules:
                dialogueLines = bendingTheRulesStepDialogue;
            break;

            case DemoStepName.MoreRewardsThanUsual:
                dialogueLines = moreRewardsThanUsualStepDialogue;
            break;

            case DemoStepName.StartOfFirstStaticChooseNShop:
                dialogueLines = startOfFirstStaticChooseNShopDialogue;
            break;

            case DemoStepName.StartOfSecondStaticChooseNShop:
                dialogueLines = startOfSecondStaticChooseNShopDialogue;
            break;

            case DemoStepName.BuyCompanionReminder:
                dialogueLines = buyCompanionReminderStepDialogue;
            break;

            case DemoStepName.FullFeatureShopTutorialStep1:
                dialogueLines = secondShopTutorialStep1Dialogue;
            break;

            case DemoStepName.FullFeatureShopTutorialStep2:
                dialogueLines = secondShopTutorialStep2Dialogue;
            break;

            case DemoStepName.FullFeatureShopTutorialStep3:
                dialogueLines = secondShopTutorialStep3Dialogue;
            break;

            case DemoStepName.FullFeatureShopTutorialStep4:
                dialogueLines = secondShopTutorialStep4Dialogue;
            break;

            case DemoStepName.PostCombatRewardsDialogue:
                dialogueLines = postCombatRewardsDialogueDialogue;
            break;

            case DemoStepName.PrematureEndTurnReminder:
                dialogueLines = prematureEndTurnReminderDialogueLines;
            break;

            case DemoStepName.YouShouldSpendYourMoneyBozo:
                dialogueLines = youShouldSpendYourMoneyBozoDialogueLines;
            break;

            case DemoStepName.BoosterPackShopTutorialStep1:
                dialogueLines = boosterPacksShopTutorialStep1Dialogue;
            break;

            case DemoStepName.BoosterPackShopTutorialStep2:
                dialogueLines = boosterPacksShopTutorialStep2Dialogue;
            break;

            case DemoStepName.BoosterPackShopTutorialStep3:
                dialogueLines = boosterPacksShopTutorialStep3Dialogue;
            break;

            case DemoStepName.BoosterPackShopTutorialStep4:
                dialogueLines = boosterPacksShopTutorialStep4Dialogue;
            break;
        }

        yield return GenericDemoStepCoroutine(demoStepName, dialogueLines);
    }

    private IEnumerator GenericDemoStepCoroutine(DemoStepName demoStepName, List<string> dialogueLines) {
        foreach (string line in dialogueLines) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[demoStepName] = true;
    }

    public IEnumerator CombatTutorialStep() {
        IsCombatOnboardingActive = true;
        yield return CombatOnboardingDirector.Instance.RunAllStepsCoroutine();
        IsCombatOnboardingActive = false;
        gameState.HasSeenCombatTutorial = true;
        demoDataSO.stepCompletion[DemoStepName.CombatTutorialStep] = true;
    }

    private IEnumerator CardBuyingTutorial() {
        ShopManager.Instance.shopViewController.DisableAllUIPreserveAppearance();
        foreach (string line in cardBuyingTutorialStep1Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        yield return new WaitForSeconds(1f);
        ShopManager.Instance.shopViewController.EnableAllUI();
        MultiDeckViewManager.Instance.ShowShopDeckView();
        MultiDeckViewManager.Instance.DisableInteractions();
        yield return new WaitForSeconds(1f);
        foreach (string line in cardBuyingTutorialStep2Dialogue) {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
        demoDataSO.stepCompletion[DemoStepName.CardBuyingTutorial] = true;
        MultiDeckViewManager.Instance.EnableInteractions();
    }
}

public enum DemoStepName {
    BendingTheRules,
    MoreRewardsThanUsual,
    StartOfFirstStaticChooseNShop,
    BuyCompanionReminder,
    CardBuyingTutorialStep1, // Deprecated
    FullFeatureShopTutorialStep1,
    FullFeatureShopTutorialStep2,
    FullFeatureShopTutorialStep3,
    FullFeatureShopTutorialStep4,
    PostCombatRewardsDialogue,
    StartOfSecondStaticChooseNShop,
    PrematureEndTurnReminder,
    CombatTutorialStep,
    YouShouldSpendYourMoneyBozo,
    CardBuyingTutorialStep2, // Deprecated
    CardBuyingTutorial,
    BoosterPackShopTutorialStep1,
    BoosterPackShopTutorialStep2,
    BoosterPackShopTutorialStep3,
    BoosterPackShopTutorialStep4,
}