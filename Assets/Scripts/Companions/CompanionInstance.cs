using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CompanionAbilityInvoker))]
public class CompanionInstance : CombatEntityWithDeckInstance 
{
    public Companion companion;
    private CompanionAbilityInvoker abilityInvoker;

    protected override void Start() {
        base.Start();
        abilityInvoker = GetComponent<CompanionAbilityInvoker>();
    }

    protected override IEnumerator onDeath()
    {
        
        abilityInvoker.onDeath();
        return base.onDeath();
    }

    public override bool isTargetableByChildImpl(EffectTargetRequestEventInfo eventInfo)
    {
        // TODO: figure out a way to prevent companions from drawing from an empty deck
        // like so but with a check before to see if it's a draw effect:
        /*
        return inCombatDeck.drawPile.Count > 0 
            || inCombatDeck.discardPile.Count > 0;
            */
        return true;
    }

    public void cardEffectEventHandler(CardEffectEventInfo info){
        if(!info.targets.Contains(this)) return;
        switch(info.effectName) {
            case SimpleEffectName.Draw:
                dealCards(info.scale);
                break;
            case SimpleEffectName.Damage:
                // TODO: heal effect
                stats.currentHealth -= info.scale;
                break;
            case SimpleEffectName.Buff:
                base.applyStatusEffect(StatusEffect.Strength, info.scale);
                break;
            case SimpleEffectName.Weaken:
                base.applyStatusEffect(StatusEffect.Weakness, info.scale);
                break;
            case SimpleEffectName.Discard:
                Debug.LogWarning("Oh god a companion is getting discarded what happened");
                break;
        }
    }

}

