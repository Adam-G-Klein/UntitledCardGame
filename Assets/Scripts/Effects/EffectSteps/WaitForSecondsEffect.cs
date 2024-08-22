using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This effect step waits for float seconds
*/
public class WaitForSecondsEffect : EffectStep
{
    [SerializeField]
    private float seconds = 0;

    public WaitForSecondsEffect() {
        effectStepName = "WaitForSecondsEffect";
    }

    public override IEnumerator invoke(EffectDocument document) {
        yield return new WaitForSeconds(seconds);
    }
}