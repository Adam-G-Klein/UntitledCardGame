using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectProcedureContext {
    public CardCastManager cardCastManager;
    public CompanionManager companionManager;
    public EnemyManager enemyManager;
    public CombatEntityInEncounterStats casterStats;
    public CombatEntityWithDeckInstance cardCaster;
    public PlayerHand playerHand;
    public List<TargettableEntity> alreadyTargetted;

    
    public EffectProcedureContext(CardCastManager caster, 
        CompanionManager companionManager, 
        EnemyManager enemyManager, 
        CombatEntityWithDeckInstance cardCaster, 
        PlayerHand playerHand, 
        List<TargettableEntity> alreadyTargetted) {
        this.cardCastManager = caster;
        this.companionManager = companionManager;
        this.enemyManager = enemyManager;
        this.cardCaster = cardCaster;
        this.casterStats = cardCaster.stats;
        this.playerHand = playerHand;
        this.alreadyTargetted = alreadyTargetted;
    }
    
}
[System.Serializable]
public abstract class EffectProcedure: TargetRequester
{
    
    protected  EffectProcedureContext context;

    public virtual void resetCastingState() {}

    protected void raiseSimpleEffect(SimpleEffectName simpleEffectName, int scale, List<TargettableEntity> targets) {
        if(context == null) Debug.LogError("Need procedure context to raiseSimpleEffect, be sure to set the 'context' field of this procedure (in the parent class) before proceeding to code it");
        context.cardCastManager.raiseSimpleEffect(simpleEffectName, scale, targets);
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