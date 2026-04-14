using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum CombatOnboardingStepName
{
    CombatIntroduction,
}

public class CombatOnboardingDirector : GenericSingleton<CombatOnboardingDirector>
{
    public Sprite speakerSprite;

    [TextArea]
    public List<string> combatIntroductionDialogue;

    private List<CombatOnboardingStepName> stepOrder = new List<CombatOnboardingStepName>()
    {
        CombatOnboardingStepName.CombatIntroduction,
    };

    public IEnumerator RunAllStepsCoroutine()
    {
        foreach (CombatOnboardingStepName stepName in stepOrder)
        {
            yield return InvokeCombatOnboardingStepCoroutine(stepName);
        }
    }

    public IEnumerator InvokeCombatOnboardingStepCoroutine(CombatOnboardingStepName stepName)
    {
        switch (stepName)
        {
            case CombatOnboardingStepName.CombatIntroduction:
                yield return CombatIntroductionStep();
            break;
        }
        yield return null;
    }

    public IEnumerator CombatIntroductionStep()
    {
        foreach (string line in combatIntroductionDialogue)
        {
            yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, line, true);
        }
        DialogueView.Instance.Hide();
    }
}
