using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
// Thinking we should just take in the procedure context rather than extending it
public class SimpleEffectArguments: EffectProcedureContext {
    public SimpleEffectArguments (CardCaster caster, CompanionManager companionManager, EnemyManager enemyManager): 
        base(caster, companionManager, enemyManager){}
}
*/
[System.Serializable]
public class SimpleEffect: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public string procedureClass;
    public SimpleEffectName effectName;
    public int baseScale = 0;
    public bool targetAllValidTargets = false;
    public List<EntityType> validTargets;
    [Tooltip("Specifies whether this effect must have a different target "
        + "from other effects in the card. Example: a series of discard "
        + "effects that each need to target a different card.")]
    public bool requiresUniqueTarget;

    public SimpleEffect() {
        procedureClass = "SimpleEffect";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        if(targetAllValidTargets) {
            currentTargets.AddRange(context.caster.getAllValidTargets(validTargets));
        }
        else {
            context.caster.requestTarget(validTargets, this,
                requiresUniqueTarget ? context.alreadyTargetted : null);
        }
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
        // passes back to the cardCaster, where it will call invoke
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        context.caster.raiseSimpleEffect(
            effectName, 
            context.caster.getEffectScale(effectName, baseScale),
            currentTargets);
        yield return null;
    }

    public override void resetCastingState(){
        currentTargets.Clear();
    }

}