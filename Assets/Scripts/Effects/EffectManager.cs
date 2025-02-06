using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EffectManager : GenericSingleton<EffectManager>
{
    public bool interruptEffectWorkflow = false;
    // Invoke for calculation effect workflows SHOULD NOT interfere with
    // normal ongoing effects.
    public bool interruptEffectWorkflowForCalculation = false;
    private IEnumerator currentEffectWorkflow;
    private IEnumerator currentEffectStep;
    private List<EffectWorkflowClosure> effectWorkflowQueue;

    private bool effectRunning = false;

    void Awake() {
        effectWorkflowQueue = new List<EffectWorkflowClosure>();
    }

    public bool IsEffectRunning() {
        return effectRunning;
    }

    public void invokeEffectWorkflow(
            EffectDocument document,
            List<EffectStep> effectSteps,
            IEnumerator callback) {
        Debug.Log("Invoking effect workflow via sync call");
        currentEffectWorkflow = effectWorkflowCoroutine(document, effectSteps, callback);
        StartCoroutine(currentEffectWorkflow);
    }
    public void invokeEffectWorkflow(
            EffectDocument document,
            EffectWorkflow workflow,
            IEnumerator callback) {
        Debug.Log("Invoking effect workflow via sync call");
        currentEffectWorkflow = effectWorkflowCoroutine(document, workflow.effectSteps, callback);
        StartCoroutine(currentEffectWorkflow);
    }

    public void invokeEffectWorkflowForCalculation(
            EffectDocument document,
            List<EffectStep> effectSteps,
            IEnumerator callback) {
        Debug.Log("Invoking effect workflow for calculation via sync call");
        StartCoroutine(effectWorkflowCoroutineForCalculation(document, effectSteps, callback));
    }

    public void CancelEffectWorkflow() {
        Debug.Log("EffectManager: Stopping effect coroutines");
        StopCoroutine(currentEffectStep);
        StopCoroutine(currentEffectWorkflow);
        effectRunning = false;
    }

    public void QueueEffectWorkflow(EffectWorkflowClosure workflow) {
        Debug.Log("Queueing up an effect workflow with " + workflow.flow.effectSteps.Count + " effect steps");
        workflow.document.map.Print();
        effectWorkflowQueue.Add(workflow);
    }

    public IEnumerator invokeEffectWorkflowCoroutine(
            EffectDocument document,
            List<EffectStep> effectSteps,
            IEnumerator callback) {
        Debug.Log("Invoking effect workflow via coroutine");
        currentEffectWorkflow = effectWorkflowCoroutine(document, effectSteps, callback);
        yield return StartCoroutine(currentEffectWorkflow);
    }

    private IEnumerator effectWorkflowCoroutine(
            EffectDocument document,
            List<EffectStep> effectSteps,
            IEnumerator callback) {
        effectRunning = true;
        foreach (EffectStep step in effectSteps) {
            if (interruptEffectWorkflow) {
                Debug.Log("Breaking from workflow");
                interruptEffectWorkflow = false;
                break;
            }
            Debug.Log("Invoking Step [" + step.effectStepName + "]");
            currentEffectStep = step.invoke(document);
            yield return StartCoroutine(currentEffectStep);
        }

        if (callback != null) yield return StartCoroutine(callback);

        // If the previous effect worklfow queue'd up a new one, then execute the new one
        if (effectWorkflowQueue.Count > 0) {
            Debug.Log("Kicking off queued effect workflow");
            EffectWorkflowClosure workflow = effectWorkflowQueue[0];
            effectWorkflowQueue.RemoveAt(0);
            // workflow.document.map.Print();
            currentEffectWorkflow = effectWorkflowCoroutine(workflow.document, workflow.flow.effectSteps, workflow.callback);
            StartCoroutine(currentEffectWorkflow);
        } else {
            effectRunning = false;
        }

        yield return null;
    }

    // effectWorkflowCoroutineForCalculation runs a coroutine for calculation purposes.
    // These SHOULD NOT be side-effect-ey, so they should not trigger other effects in the
    // game such as enemy abilities or companion abilities.
    // Therefore, we do not execute queued up effect workflows or mess with the "effectRunning"
    // global variable.
    // The state for this should be kept separate from the mainstream way for running effect workflows.
    private IEnumerator effectWorkflowCoroutineForCalculation(
        EffectDocument document,
        List<EffectStep> effectSteps,
        IEnumerator callback) {
        bool hasEndWorkflowCheck = false;
        bool didBreak = false;
        foreach (EffectStep step in effectSteps) {
            if (interruptEffectWorkflowForCalculation) {
                Debug.Log("Breaking from workflow");
                interruptEffectWorkflowForCalculation = false;
                didBreak = true;
                break;
            }
            if (step is IEffectStepCalculation) {
                if (step is EndWorkflowIfConditionMet) {
                    hasEndWorkflowCheck = true;
                }
                Debug.Log("CALCULATION: Invoking Step [" + step.effectStepName + "]");
                currentEffectStep = ((IEffectStepCalculation)step).invokeForCalculation(document);
                yield return StartCoroutine(currentEffectStep);
            }
        }

        if(!document.boolMap.ContainsKey("highlightCard")){
            document.boolMap.Add("highlightCard", hasEndWorkflowCheck && !didBreak);
        }

        if (callback != null) yield return StartCoroutine(callback);

        yield return null;
    }
}
