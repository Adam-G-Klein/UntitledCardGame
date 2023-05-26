using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : GenericSingleton<EffectManager>
{
    private IEnumerator currentEffectWorkflow;
    private IEnumerator currentEffectStep;

    public void invokeEffectWorkflow(
            EffectDocument document,
            List<EffectStep> effectSteps,
            Action callback) {
        currentEffectWorkflow = effectWorkflowCoroutine(document, effectSteps, callback);
        StartCoroutine(currentEffectWorkflow);
    }

    private IEnumerator effectWorkflowCoroutine(
            EffectDocument document,
            List<EffectStep> effectSteps,
            Action callback) {
        foreach (EffectStep step in effectSteps) {
            currentEffectStep = step.invoke(document);
            yield return StartCoroutine(currentEffectStep);
        }
        callback();
        yield return null;
    }
}
