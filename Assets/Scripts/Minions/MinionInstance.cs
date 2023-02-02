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

    // So torn on whether to try to combine this with enemy effect handler
    public void cardEffectEventHandler(CardEffectEventInfo info){
        if(!info.targets.Contains(this)) return;
        base.applyStatusEffects(info.statusEffects);
        switch(info.effectName) {
            case SimpleEffectName.Draw:
                dealCards(info.scale);
                break;
            case SimpleEffectName.Damage:
                // TODO: heal effect
                stats.currentHealth -= info.scale;
                break;
            case SimpleEffectName.Discard:
                Debug.LogWarning("Oh god a minion is getting discarded what happened");
                break;
        }
    }

}

