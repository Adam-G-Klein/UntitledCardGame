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
public class MultiStrike: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public string procedureClass;
    public int numStrikes;
    public int baseScale = 0;
    public float strikeDelay = 0.2f;
    public List<EntityType> validTargets = new List<EntityType>() {EntityType.Enemy};

    public MultiStrike() {
        procedureClass = "MultiStrike";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        //args.context.caster.raiseSimpleEffect(simpleEffectName);
        context.caster.requestTarget(validTargets, this);
        yield return new WaitUntil(() => currentTargets.Count > 0);
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        for(int i = 0; i < numStrikes; i++) {
            context.caster.raiseSimpleEffect(
                SimpleEffectName.Damage, 
                context.caster.getEffectScale(SimpleEffectName.Damage, baseScale),
                currentTargets);
            yield return new WaitForSeconds(strikeDelay);
        }
    }


    public override void resetCastingState(){
        currentTargets.Clear();
    }


}