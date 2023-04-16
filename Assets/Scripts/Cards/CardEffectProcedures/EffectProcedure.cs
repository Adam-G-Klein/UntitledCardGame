using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectProcedureType {
    Bellows,
    BodySlam,
    Brainstorm,
    CardEffectProcedure,
    CombatEffectProcedure,
    EffectForStatusCards,
    EffectIncreaseOnPlay,
    IntEffectProcedure,
    MultiStrike,
    Mutate,
    PurifyingInferno,
    ShuffleIn,
    SpawnMinions,
    Taunt
}

public class EffectProcedureContext {
    public CardCastManager cardCastManager;
    public CompanionManager companionManager;
    public EnemyManager enemyManager;
    public CardSelectionManager cardSelectionManager;
    public CombatEntityInEncounterStats casterStats;
    public CombatEntityWithDeckInstance cardCaster;
    public CombatEffectEvent combatEffectEvent;
    public PlayerHand playerHand;
    public List<TargettableEntity> alreadyTargetted;
    // Here just so that EffectIncreasesOnPlay can modify the card
    public Card castingCard;

    
    public EffectProcedureContext(CardCastManager caster, 
        CompanionManager companionManager, 
        EnemyManager enemyManager, 
        CombatEntityWithDeckInstance cardCaster, 
        PlayerHand playerHand, 
        List<TargettableEntity> alreadyTargetted, 
        CombatEffectEvent combatEffectEvent,
        CardSelectionManager cardSelectionManager,
        Card castingCard) {
        this.cardCastManager = caster;
        this.companionManager = companionManager;
        this.enemyManager = enemyManager;
        this.cardCaster = cardCaster;
        this.casterStats = cardCaster.stats;
        this.playerHand = playerHand;
        this.alreadyTargetted = alreadyTargetted;
        this.combatEffectEvent = combatEffectEvent;
        this.cardSelectionManager = cardSelectionManager;
        this.castingCard = castingCard;
    }
    
}
[System.Serializable]
public abstract class EffectProcedure: TargetRequester
{

    public string procedureClass;
    
    protected  EffectProcedureContext context;

    public virtual void resetCastingState() {
        resetTargets();
    }

    // Called before the procedure is invoked to allow the procedure to
    // get targets, do any math it needs to, be ready to raise its events
    public virtual IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        yield return null;
    }

    // Called after prepare, where the procedure raises effects now that it 
    // has all the information/targets it needs
    public virtual IEnumerator invoke(EffectProcedureContext context) {
        yield return null;
    }

}