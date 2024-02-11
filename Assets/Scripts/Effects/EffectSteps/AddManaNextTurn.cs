using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddManaNextTurn : EffectStep
{
    [SerializeField]
    private int manaToAdd = 0;

    public override IEnumerator invoke(EffectDocument document) {
        ManaManager.Instance.AddExtraManaNextTurn(manaToAdd);
        yield return null;
    }
}
