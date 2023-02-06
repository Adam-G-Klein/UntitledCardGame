using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Taunt: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public const string description = "Taunt an enemy to attack the casting companion instead of its current target";
    public int baseScale = 1;
    public List<EntityType> validTargets = new List<EntityType>() {EntityType.Enemy};

    public Taunt() {
        procedureClass = "Taunt";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        context.cardCastManager.requestTarget(validTargets, this);
        Debug.Log("Taunt target requested, valid targets: " + validTargets.Count + " valid target: " + (validTargets[0] == EntityType.Enemy ? "Enemy" : "not enemy"));
        yield return new WaitUntil(() => currentTargets.Count > 0);
        Debug.Log("Taunt target acquired." + currentTargets[0].GetType());
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        
        // Is this a train wreck if our targetting system messes up? yes
        // Am I choosing to trust it? also yes
        EnemyInstance enemy = ((EnemyInstance) currentTargets[0]);
        enemy.setTauntedTarget(context.cardCaster);
        yield return null;
    }

    public override void resetCastingState(){
        currentTargets.Clear();
    }

}