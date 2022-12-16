using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class EffectProcedureOutput {
    // Zero chance this is the final iteration of this class
    // We will probably want something better than a big data
    // class to throw around
    public int cardsDrawn;
    // Idea would be we have other pieces of info further procedures might
    // want to know about down here
}
*/

public class EffectProcedureContext {
    public CardCaster caster;
    public CompanionManager companionManager;
    public EnemyManager enemyManager;
    public CombatEntityInEncounterStats stats;

    public EffectProcedureContext (CardCaster caster, 
        CompanionManager companionManager, 
        EnemyManager enemyManager,
        CombatEntityInEncounterStats stats) {
        this.caster = caster;
        this.companionManager = companionManager;
        this.enemyManager = enemyManager;
        this.stats = stats;

    }
}
[System.Serializable]
public abstract class EffectProcedure
{
    
    protected  EffectProcedureContext context;
    protected List<string> getTarget(List<EntityType> validTargets, bool getAllPossibleTargets){
        if(context == null) Debug.LogError("Need procedure context to get targets, be sure to set the 'context' field of this procedure (in the parent class) before proceeding to code it");
        // context.caster.get
        return null;

    }

    public abstract void targetsSupplied(List<string> targets);

    protected void raiseSimpleEffect(SimpleEffectName simpleEffectName, int scale, List<string> targets) {
        if(context == null) Debug.LogError("Need procedure context to raiseSimpleEffect, be sure to set the 'context' field of this procedure (in the parent class) before proceeding to code it");
        context.caster.raiseSimpleEffect(simpleEffectName, scale, targets);
    }

    // Would change to returning EffectProcedureOutput if we decide we need it
    public abstract IEnumerator invoke(EffectProcedureContext context);

}