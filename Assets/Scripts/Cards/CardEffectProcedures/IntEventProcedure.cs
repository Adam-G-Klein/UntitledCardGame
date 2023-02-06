using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntEffectProcedure: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public const string description = "Raise an int event. Example: Raise a mana change";
    public IntGameEvent gameEvent;
    public int baseScale = 0;

    public IntEffectProcedure() {
        procedureClass = "IntEffectProcedure";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        yield return null;
        // passes back to the cardCaster, where it will call invoke
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        context.cardCastManager.raiseIntEvent(gameEvent, baseScale);
        yield return null;
    }

}