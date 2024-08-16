using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
/*
What information do we need to draw the view:
- all the CombatInstance's health
- all the CombatInstance's statuses
- the cards in draw and discard for all decks


What can trigger a change in the view
- someone takes damage
- someone has a status applied
- ANYTHING happens with cards
- An EffectWorkflow finishes
- An enemy intent is updated


*/

public class EnemyEncounterViewModel : GenericSingleton<EnemyEncounterViewModel> 
{

    private CombatEncounterView listener;
    public List<EnemyInstance> enemies;
    public List<CompanionInstance> companions;

    public void SetStateDirty() {
        listener.UpdateView();
    }

    public void SetListener(CombatEncounterView listener) {
        this.listener = listener;
    }

    private DeckInstance GetDeckInstanceFromCompanionInstance(CompanionInstance companionInstance){
        // guaranteed by RequireComponent
        return companionInstance.GetComponent<DeckInstance>();
    }

    private Dictionary<StatusEffect, int> GetStatusEffects(CombatInstance combatInstance){
        return combatInstance.statusEffects;
    }

}