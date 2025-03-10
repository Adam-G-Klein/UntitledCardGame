using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
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
    // Initialized by the enemyEncounterManager after the encounter is built
    public List<EnemyInstance> enemies; 
    // Initialized by the enemyEncounterManager after the encounter is built
    public List<CompanionInstance> companions;

    private bool updatingAtEndOfFrame = false;

    public VisualElement hoveredElement;
    public IUIEntity hoveredEntity;

    public void SetStateDirty() {
        if(!updatingAtEndOfFrame) {
            StartCoroutine(updateViewAtEndOfFrame());
        } 
    }

    public void SetInMenu(bool isInMenu) {
        if (listener) {
            listener.SetInMenu(isInMenu);
            SetStateDirty();
        }
    }

    public void SetInDeckView(bool isInDeckView) {
        if (listener) {
            listener.SetInDeckView(isInDeckView);
            SetStateDirty();
        }
    }

    private IEnumerator updateViewAtEndOfFrame() {
        updatingAtEndOfFrame = true;
        yield return new WaitForEndOfFrame();
        listener.UpdateView();
        updatingAtEndOfFrame = false;
    }

    public void SetListener(CombatEncounterView listener) {
        this.listener = listener;
    }

    private DeckInstance GetDeckInstanceFromCompanionInstance(CompanionInstance companionInstance){
        // guaranteed by RequireComponent
        return companionInstance.GetComponent<DeckInstance>();
    }

    private Dictionary<StatusEffectType, int> GetStatusEffects(CombatInstance combatInstance){
        return combatInstance.GetStatusEffects();
    }

}