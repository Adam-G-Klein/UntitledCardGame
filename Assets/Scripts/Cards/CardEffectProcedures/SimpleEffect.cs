using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimpleEffectName {
    Draw,
    Damage,
    Strength,
    Discard,
    Weaken,
    Defend
}
[System.Serializable]
public class SimpleEffect: EffectProcedure {
    // Causes the whole class to serialize differently if this field 
    // has a default value. *shrug*
    public string procedureClass;
    public SimpleEffectName effectName;
    public int baseScale = 0;
    public bool targetCaster = false;
    public bool requiresUniqueTarget = false;
    public bool targetAllValidTargets = false;
    public List<EntityType> validTargets;
    [Tooltip("Specifies whether this effect must have a different target "
        + "from other effects in the card. Example: a series of discard "
        + "effects that each need to target a different card.")]


    public SimpleEffect() {
        procedureClass = "SimpleEffect";
    }
    
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        if(targetAllValidTargets) {
            currentTargets.AddRange(context.cardCastManager.getAllValidTargets(validTargets));
        } else if(targetCaster) {
            currentTargets.Add(context.cardCaster);
        }
        else {
            context.cardCastManager.requestTarget(validTargets, this,
                requiresUniqueTarget ? context.alreadyTargetted : null);
        }
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
        // passes back to the cardCaster, where it will call invoke
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        context.cardCastManager.raiseSimpleEffect(
            effectName, 
            getEffectScale(effectName, context.casterStats, baseScale),
            currentTargets);
        yield return null;
    }

    public override void resetCastingState(){
        currentTargets.Clear();
    }

    // We don't know what entityStat we need to be querying unless we use a 
    // switch case like this. Other effectProcedures should just grab the stat they need
    // from EncounterStats
    public static int getEffectScale(SimpleEffectName effect, CombatEntityInEncounterStats stats, int baseScale) {
        switch(effect) {
            case SimpleEffectName.Draw:
                // no stat affecting draw yet
                return baseScale;
            case SimpleEffectName.Damage:
                // use the getter from the stats object
                return baseScale + stats.currentAttackDamage;
            case SimpleEffectName.Strength:
                // no stat affecting buffing yet
                return baseScale;
            case SimpleEffectName.Defend:
                // no stat affecting Defending yet
                return baseScale;
            default:
                return baseScale;
        }
    }

}