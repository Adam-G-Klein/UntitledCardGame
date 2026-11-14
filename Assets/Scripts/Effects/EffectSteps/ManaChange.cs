using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The effect that deals with changing mana

    Input: NA
    Output: NA
    Parameters:
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class ManaChange : EffectStep
{
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";

    public ManaChange() {
        effectStepName = "ManaChange";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }

        ManaManager.Instance.updateMana(finalScale);
        // show where the Élan came from: ink trails from the card played (or the rat) into the emblem
        if (finalScale > 0) {
            Component from = document.map.ContainsValueWithKey<PlayableCard>(EffectDocument.ORIGIN) ? document.map.TryGetItem<PlayableCard>(EffectDocument.ORIGIN, 0)
                : document.map.ContainsValueWithKey<CompanionInstance>(EffectDocument.ORIGIN) ? (Component)document.map.TryGetItem<CompanionInstance>(EffectDocument.ORIGIN, 0) : null;
            if (from != null) ManaDisplayVFX.PlayGainFrom(from.transform.position);
        }

        yield return null;
    }
}
