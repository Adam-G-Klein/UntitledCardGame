using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : GenericSingleton<EffectManager>
{
    public bool interruptEffectWorkflow = false;
    private IEnumerator currentEffectWorkflow;
    private IEnumerator currentEffectStep;
    private List<EffectWorkflow> effectWorkflowQueue;

    void Awake() {
        effectWorkflowQueue = new List<EffectWorkflow>();
    }

    public void invokeEffectWorkflow(
            EffectDocument document,
            List<EffectStep> effectSteps,
            Action callback) {
        Debug.Log("Invoking effect workflow via sync call");
        currentEffectWorkflow = effectWorkflowCoroutine(document, effectSteps, callback);
        StartCoroutine(currentEffectWorkflow);
    }

    public void CancelEffectWorkflow() {
        Debug.Log("EffectManager: Stopping effect coroutines");
        StopCoroutine(currentEffectStep);
        StopCoroutine(currentEffectWorkflow);
    }

    public void QueueEffectWorkflow(EffectWorkflow workflow) {
        effectWorkflowQueue.Add(workflow);
    }

    public IEnumerator invokeEffectWorkflowCoroutine(
            EffectDocument document,
            List<EffectStep> effectSteps,
            Action callback) {
        Debug.Log("Invoking effect workflow via coroutine");
        currentEffectWorkflow = effectWorkflowCoroutine(document, effectSteps, callback);
        yield return StartCoroutine(currentEffectWorkflow);
    }

    private IEnumerator effectWorkflowCoroutine(
            EffectDocument document,
            List<EffectStep> effectSteps,
            Action callback) {
        foreach (EffectStep step in effectSteps) {
            if (interruptEffectWorkflow) {
                Debug.Log("Breaking from workflow");
                interruptEffectWorkflow = false;
                break;
            }
            Debug.Log("Invoking Step");
            currentEffectStep = step.invoke(document);
            yield return StartCoroutine(currentEffectStep);
        }

        if (callback != null) callback();

        // If the previous effect worklfow queue'd up a new one, then execute the new one
        if (effectWorkflowQueue.Count > 0) {
            Debug.Log("Kicking off queued effect workflow");
            EffectWorkflow workflow = effectWorkflowQueue[0];
            effectWorkflowQueue.RemoveAt(0);
            document.originEntityType = EntityType.Unknown;
            currentEffectWorkflow = effectWorkflowCoroutine(document, workflow.effectSteps, null);
            StartCoroutine(currentEffectWorkflow);
        }

        yield return null;
    }
}
