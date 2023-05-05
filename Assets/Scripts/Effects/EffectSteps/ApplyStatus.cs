using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Unimplemented
*/
public class ApplyStatus : EffectStep
{
    [SerializeField]
    [Tooltip(
        "Designates what entity to add status to based on " + 
        "the key the entity was saved to from a previous effect"
    )]
    private string inputKey = "";
    [SerializeField]
    private StatusEffect statusEffect;
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";

    public ApplyStatus() {
        effectStepName = "ApplyStatus";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        yield return null;
    }
}