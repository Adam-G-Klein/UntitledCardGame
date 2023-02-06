using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DisplayedCombatEffect {
    Damage,
    Strengthen,
    Weaken,
    ApplyDefended,
    DrawFrom,
    SetHealth,
    Sacrifice,
    AddToDamageMultiply,
    // for things like fixed damage effects
    FixedDamage,
    ApplyInvulnerability,
    Heal,
    ApplyMaxHpBounty
}
[System.Serializable]
public class CombatEffectProcedure: EffectProcedure {

    public DisplayedCombatEffect effectName;
    private CombatEffect internalEffectName;

    private Dictionary<DisplayedCombatEffect, CombatEffect> displayedToCombatEffect = new Dictionary<DisplayedCombatEffect, CombatEffect>() {
        {DisplayedCombatEffect.Damage, CombatEffect.Damage},
        {DisplayedCombatEffect.Strengthen, CombatEffect.Strength},
        {DisplayedCombatEffect.Weaken, CombatEffect.Weakness},
        {DisplayedCombatEffect.ApplyDefended, CombatEffect.Defended},
        {DisplayedCombatEffect.DrawFrom, CombatEffect.DrawFrom},
        {DisplayedCombatEffect.SetHealth, CombatEffect.SetHealth},
        {DisplayedCombatEffect.Sacrifice, CombatEffect.Sacrifice},
        {DisplayedCombatEffect.AddToDamageMultiply, CombatEffect.AddToDamageMultiply},
        {DisplayedCombatEffect.FixedDamage, CombatEffect.Damage},
        {DisplayedCombatEffect.ApplyInvulnerability, CombatEffect.ApplyInvulnerability},
        {DisplayedCombatEffect.Heal, CombatEffect.Heal},
        {DisplayedCombatEffect.ApplyMaxHpBounty, CombatEffect.ApplyMaxHpBounty}
    };
    public int baseScale = 0;
    public bool targetCaster = false;
    public bool targetAllValidTargets = false;
    public int targetToUse = -1;
    public List<EntityType> validTargets;

    public CombatEffectProcedure() {
        procedureClass = "CombatEffectProcedure";
    }
    
    // Yeah, we should probably find a way to make this more readable.
    // at least it's centralized for now *shrug*
    public override IEnumerator prepare(EffectProcedureContext context) {
        this.context = context;
        resetCastingState();
        internalEffectName = displayedToCombatEffect[effectName];
        if(validTargets.Contains(EntityType.Card)) {
            Debug.LogError("Ah sorry friend. You can't target cards with combat effects. They're technically entities on the software side, and they need to listen to entity events, which is why they show up in that drop down :/ Just go ahead and use CardEffectProcedure instead :)");
            yield break;
        }
        if(!effectNeedsTargets(internalEffectName)) {
            yield break;
        } else if (targetToUse >= 0) {
            // TODO: make this work with effects that target multiple entities
            try {
                currentTargets.Add(context.alreadyTargetted[targetToUse]);
            } catch {
                Debug.LogError("targetToUse wasn't found in the alreadyTargetted list, either set it to -1 or a value lower than the cardinality of this effect");
            }
        } else if(targetAllValidTargets) {
            currentTargets.AddRange(context.cardCastManager.getAllValidTargets(validTargets));
        } else if(targetCaster) {
            currentTargets.Add(context.cardCaster);
        } else {
            context.cardCastManager.requestTarget(validTargets, this);
        }
        yield return new WaitUntil(() => currentTargets.Count > 0);
        context.alreadyTargetted.AddRange(currentTargets);
        // passes back to the cardCaster, where it will call invoke
    }

    public override IEnumerator invoke(EffectProcedureContext context)
    {
        context.cardCastManager.raiseCombatEffect(
            new CombatEffectEventInfo(
                new Dictionary<CombatEffect, int> {
                    {internalEffectName, 
                        // Feed the displayed name into the overloaded effectScale provider
                        // in the entitystats
                        context.casterStats.getEffectScale(effectName, baseScale)}
                },
                currentTargets,
                context.cardCaster
            )
        );
        yield return null;
    }

    public bool effectNeedsTargets(CombatEffect effect) {
        // everything needs targets right now,
        // even if it's just the caster or the full set 
        // of valid targets from the cardCastManager
        return true;
    }

}