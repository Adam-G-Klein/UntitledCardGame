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
    public CombatEntityInEncounterStats casterStats;
    public PlayerHand playerHand;
    public List<TargettableEntity> alreadyTargetted;

    public EffectProcedureContext(CardCaster caster, CompanionManager companionManager, EnemyManager enemyManager, CombatEntityInEncounterStats casterStats, PlayerHand playerHand, List<TargettableEntity> alreadySelectedTargets) {
        this.caster = caster;
        this.companionManager = companionManager;
        this.enemyManager = enemyManager;
        this.casterStats = casterStats;
        this.playerHand = playerHand;
        this.alreadyTargetted = alreadySelectedTargets;
    }
    
}
[System.Serializable]
public abstract class EffectProcedure
{
    
    protected  EffectProcedureContext context;

    public void targetsSupplied(List<TargettableEntity> targets) {
        this.targets = targets;
    }

    public virtual void resetCastingState() {}

    protected List<TargettableEntity> targets = new List<TargettableEntity>();
    protected void raiseSimpleEffect(SimpleEffectName simpleEffectName, int scale, List<TargettableEntity> targets) {
        if(context == null) Debug.LogError("Need procedure context to raiseSimpleEffect, be sure to set the 'context' field of this procedure (in the parent class) before proceeding to code it");
        context.caster.raiseSimpleEffect(simpleEffectName, scale, targets);
    }

    // Called before the procedure is invoked to allow the procedure to
    // get targets, do any math it needs to, be ready to raise its events
    public abstract IEnumerator prepare(EffectProcedureContext context);

    // Called after prepare, where the procedure raises simple effects now that it 
    // has all the information/targets it needs
    public virtual IEnumerator invoke(EffectProcedureContext context) {
        yield return null;
    }

}