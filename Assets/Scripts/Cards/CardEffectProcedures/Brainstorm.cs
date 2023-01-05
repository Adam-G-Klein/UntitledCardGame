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
public class Brainstorm: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public string procedureClass;
    public int baseScale = 0;
    public List<EntityType> validTargets = new List<EntityType>() {EntityType.Enemy};
    private List<string> targets = new List<string>();

    public Brainstorm() {
        procedureClass = "Brainstorm";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        targets.Clear();
        //args.context.caster.raiseSimpleEffect(simpleEffectName);
        context.caster.requestTarget(validTargets, this);
        yield return new WaitUntil(() => targets.Count > 0);
        // Subtract 1 because we don't want to count the card we're playing
        int damage = (context.playerHand.cardsInHand.Count - 1) * context.casterStats.strength;
        context.caster.raiseSimpleEffect(
            SimpleEffectName.Damage, 
            damage,
            targets);
    }

    public override void targetsSupplied(List<string> targets){
        Debug.Log("Simple Effect targets supplied: " + targets.Count);
        this.targets.AddRange(targets);
    }

}