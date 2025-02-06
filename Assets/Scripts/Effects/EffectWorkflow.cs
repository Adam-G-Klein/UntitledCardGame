using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class EffectWorkflow
{
    [SerializeReference]
    public List<EffectStep> effectSteps;

    public EffectWorkflow(List<EffectStep> steps = null) {
        effectSteps = steps == null ? new List<EffectStep>() : new List<EffectStep>(steps);
    }

}

// EffectWorkflowClosure is an effect workflow with an instantiated EffectDocument
// and a callback.
public class EffectWorkflowClosure
{
    public EffectDocument document;
    public EffectWorkflow flow;
    public IEnumerator callback;

    public EffectWorkflowClosure(EffectDocument document, EffectWorkflow flow, IEnumerator callback) {
        this.document = document;
        this.flow = flow;
        this.callback = callback;
    }
}
