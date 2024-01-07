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

