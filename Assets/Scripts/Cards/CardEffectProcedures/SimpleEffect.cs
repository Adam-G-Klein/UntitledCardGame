using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimpleEffectName {
    Draw,
    // takes into account the caster's strength
    Damage,
    Strength,
    Discard,
    Weaken,
    Defend,
    // for things like self damage effects
    FixedDamage,
    ManaChange,
    // for effects like Death's Door
    SetHealth,
    DamageMultiply
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
        if(!effectNeedsTargets(effectName)) {
            yield break;
        }
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

    // For applying the correct modifier to the effect based
    // on the caster's stats
    // This needs a refactor. Don't know where we want to put this, but we shouldn't have 
    // half the math here (for damage multiplier, as it needs to know about the effect baseScale)
    // and half in the stats class (strength and weakness modifiers don't need to know about the basescale)
    // though I guess they do because we should be flooring the weakness at 0
    // so maybe here is the right place for it, but then this should definitely apply to combatEffects and not
    // simpleEffect. Problem is we haven't done the mapping to combat effects yet here, so we'd have to move that
    // into the card cast manager, which definitely isn't the right place for it. A getter on the stats object that 
    // is provided the base scale
    // is almost certainly the best way to go.
    // bro we probably just need separate effect procedures for card effects, combat effects, and direct event effects (like mana change)
    public static int getEffectScale(SimpleEffectName effect, CombatEntityInEncounterStats stats, int baseScale) {
        switch(effect) {
            case SimpleEffectName.Damage:
                // use the getter from the stats object
                return (baseScale + stats.currentAttackDamage) 
                    * stats.statusEffects[StatusEffect.DamageMultiply];
            default:
                return baseScale;
        }
    }

    public bool effectNeedsTargets(SimpleEffectName effect) {
        switch(effect) {
            case SimpleEffectName.ManaChange:
                return false;
            default:
                return true;
        }
    }

}