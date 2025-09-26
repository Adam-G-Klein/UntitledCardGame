using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Activate a Power on an entity.
    This activates a passive ability that lives with the combat Instance for the rest of the combat.
*/
public class ActivatePower : EffectStep
{
    [SerializeField]
    private string inputKey = "";

    [SerializeField]
    public PowerSO power;

    public ActivatePower()
    {
        effectStepName = "ActivatePower";
    }

    public override IEnumerator invoke(EffectDocument document)
    {
        List<CompanionInstance> companions = document.map.GetList<CompanionInstance>(inputKey);
        if (companions.Count == 0)
        {
            EffectError("No valid inputs for activating a power");
            yield break;
        }

        foreach (CompanionInstance target in companions)
        {
            target.ActivatePower(power);
        }
    }

    public TooltipViewModel GetTooltip()
    {
        return new TooltipViewModel("Passive", "Passive cards are rest-of-combat passive effects applied to individual companions");
    }
}
