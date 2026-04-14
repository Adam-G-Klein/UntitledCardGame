using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public string lineFor0To1Beat = "...";

    [Header("Refer to this sheet for line id associations: https://docs.google.com/spreadsheets/d/1sCZju8l5PAlu4M5Gi5D9lO3MF4LxUBD1K0KaEl6SMws/edit?usp=sharing")]
    [TextArea]
    public List<string> dialogueLines;

    public IEnumerator RunAllStepsCoroutine()
    {
        yield return InvokeCombatOnboardingStepCoroutine(CombatOnboardingStepName.Lines0To1);
    }

    public IEnumerator InvokeCombatOnboardingStepCoroutine(CombatOnboardingStepName stepName)
    {
        switch (stepName)
        {
            case CombatOnboardingStepName.Lines0To1:
                yield return Lines0To1();
            break;
        }
        yield return null;
    }

    public IEnumerator Lines0To1()
    {
        yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, dialogueLines[0], true);
        yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, lineFor0To1Beat, true);
        yield return DialogueView.Instance.SpeakLineCoroutine(speakerSprite, dialogueLines[1], true);
        DialogueView.Instance.Hide();
    }
}
