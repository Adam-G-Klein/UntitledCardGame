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

    // Walks the given workflows' effect steps and concatenates the tooltip from each step
    // that implements ITooltipProvider. shouldInclude, if given, can filter out a step's
    // tooltip based on its content (e.g. CardType's icon-description handling).
    public static TooltipViewModel GetEffectStepTooltips(
        IEnumerable<EffectWorkflow> workflows,
        Func<TooltipViewModel, bool> shouldInclude = null)
    {
        TooltipViewModel tooltip = new TooltipViewModel(empty: true);
        foreach (EffectWorkflow workflow in workflows)
        {
            if (workflow == null)
            {
                continue;
            }
            foreach (EffectStep step in workflow.effectSteps)
            {
                if (step is ITooltipProvider provider)
                {
                    TooltipViewModel stepTooltip = provider.GetTooltip();
                    if (shouldInclude != null && !shouldInclude(stepTooltip))
                    {
                        continue;
                    }
                    tooltip += stepTooltip;
                }
            }
        }
        return tooltip;
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
