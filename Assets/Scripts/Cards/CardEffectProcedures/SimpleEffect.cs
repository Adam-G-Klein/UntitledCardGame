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
    public List<EntityType> validTargets;
    private List<string> targets = new List<string>();

    public SimpleEffect() {
        procedureClass = "SimpleEffect";
    }
    
    public override IEnumerator invoke(EffectProcedureContext context) {
        this.context = context;
        targets.Clear();
        //args.context.caster.raiseSimpleEffect(simpleEffectName);
        context.caster.requestTarget(validTargets, this);
        yield return new WaitUntil(() => targets.Count > 0);
        Debug.Log("Simple Effect proceeding to raise simple effect");
        foreach(string target in targets) {
            Debug.Log("\tTarget: " + target);
        }
        context.caster.raiseSimpleEffect(
            effectName, 
            context.caster.getEffectScale(effectName, baseScale),
            targets);
    }

    public override void targetsSupplied(List<string> targets){
        Debug.Log("Simple Effect targets supplied: " + targets.Count);
        this.targets.AddRange(targets);
    }

    public override void setProcedureClassName(string procedureClassName)
    {
        procedureClass = procedureClassName;
    }

}