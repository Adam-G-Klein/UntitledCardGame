using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardEffectEventListener))]
public class MinionInstance : CombatEntityWithDeckInstance
{
    public Minion minion;

    void Awake() {
    }

    void Update()
    {
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
                Debug.LogWarning("Oh god a minion is getting discarded what happened");
                break;
        }
    }

}

