using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EffectManager : GenericSingleton<EffectManager>
{
    private Queue<EffectWorkflowClosure> effectWorkflowQueue;

    private bool effectRunning = false;

    void Awake() {
        effectWorkflowQueue = new Queue<EffectWorkflowClosure>();
    }

    public bool IsEffectRunning() {
        return effectRunning;
    }

    public void invokeEffectWorkflow(
            EffectDocument document,
            List<EffectStep> effectSteps,
            IEnumerator callback) {
        Debug.Log("Queueing effect workflow via sync call");
        QueueEffectWorkflow(new EffectWorkflowClosure(document, new EffectWorkflow(effectSteps), callback));
    }

    public void invokeEffectWorkflowForCalculation(
            EffectDocument document,
            List<EffectStep> effectSteps,
            IEnumerator callback) {
        Debug.Log("Invoking effect workflow for calculation via sync call");
        StartCoroutine(effectWorkflowCoroutineForCalculation(document, effectSteps, callback));
    }

    public void QueueEffectWorkflow(EffectWorkflowClosure workflow) {
        Debug.Log("Queueing up an effect workflow with " + workflow.flow.effectSteps.Count + " effect steps");
        workflow.document.map.Print();
        effectWorkflowQueue.Enqueue(workflow);
        if (!effectRunning) {
            StartCoroutine(ProcessEffectWorkflowQueue());
        }
    }

    // This is the only thing that should run EffectWorkflows.
    // It makes sure that child effect workflows run to completion and do not interleave.
    // Any effect workflows that are queued will be run, for example abilities that
    // are triggered by cards.
    // Note: not a stack, but a queue (Sorry MtG nerds).
    private IEnumerator ProcessEffectWorkflowQueue() {
        effectRunning = true;

        while (effectWorkflowQueue.Count > 0) {
            EffectWorkflowClosure workflow = effectWorkflowQueue.Dequeue();

            foreach (EffectStep step in workflow.flow.effectSteps) {
                if (workflow.document.workflowInterrupted) {
                    Debug.Log("Document workflow interrupted, Breaking from workflow");
                    break;
                }
                Debug.Log("Invoking Step [" + step.effectStepName + "]");
                yield return StartCoroutine(step.invoke(workflow.document));
                Debug.Log("Done with Step [" + step.effectStepName + "]");
            }

            if (workflow.document.disableCallback) {
                Debug.Log("Callback disabled for the current effect workflow");
            }

            if (workflow.callback != null && !workflow.document.disableCallback) {
                Debug.Log("Running callback for effect workflow");
                yield return StartCoroutine(workflow.callback);
                Debug.Log("Done with the callback for effect workflow");
            }
        }

        effectRunning = false;
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
        foreach (EffectStep step in effectSteps) {
            if (document.workflowInterrupted) {
                break;
            }
            if (step is IEffectStepCalculation) {
                yield return  ((IEffectStepCalculation)step).invokeForCalculation(document);;
            }
        }

        if (callback != null && !document.disableCallback) yield return callback;

        yield return null;
    }
}
