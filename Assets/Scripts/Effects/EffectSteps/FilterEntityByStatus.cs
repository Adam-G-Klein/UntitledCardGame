using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterEntityByStatus : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private StatusEffect status;
    [SerializeField]
    private string outputKey = "";

    public FilterEntityByStatus() {
        effectStepName = "FilterEntityByStatus";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> combatInstances = document.map.GetList<CombatInstance>(inputKey);
        if (combatInstances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }
        List<CombatInstance> filteredList = new List<CombatInstance>();
        foreach (CombatInstance instance in combatInstances) {
            if (instance.statusEffects[status] > 0) {
                filteredList.Add(instance);
            }
        }
        document.map.AddItems<CombatInstance>(outputKey, filteredList);
        FilterCompanionInstances(document, filteredList);
        FilterEnemyInstances(document, filteredList);
        FilterDeckInstances(document, filteredList);
        yield return null;
    }

    private void FilterCompanionInstances(
            EffectDocument document,
            List<CombatInstance> filteredCombatInstances) {
        List<CompanionInstance> companionInstances = document.map.GetList<CompanionInstance>(inputKey);
        List<CompanionInstance> filteredCompanionInstances = new List<CompanionInstance>();
        foreach (CompanionInstance instance in companionInstances) {
            if (filteredCombatInstances.Contains(instance.combatInstance)) {
                filteredCompanionInstances.Add(instance);
            }
        }
        document.map.AddItems<CompanionInstance>(outputKey, filteredCompanionInstances);
    }

    private void FilterEnemyInstances(
            EffectDocument document,
            List<CombatInstance> filteredCombatInstances) {
        List<EnemyInstance> enemyInstances = document.map.GetList<EnemyInstance>(inputKey);
        List<EnemyInstance> filteredEnemyInstances = new List<EnemyInstance>();
        foreach (EnemyInstance instance in enemyInstances) {
            if (filteredCombatInstances.Contains(instance.combatInstance)) {
                filteredEnemyInstances.Add(instance);
            }
        }
        document.map.AddItems<EnemyInstance>(outputKey, filteredEnemyInstances);
    }

    private void FilterDeckInstances(
            EffectDocument document,
            List<CombatInstance> filteredCombatInstances) {
        List<DeckInstance> deckInstances = document.map.GetList<DeckInstance>(inputKey);
        List<DeckInstance> filteredDeckInstances = new List<DeckInstance>();
        foreach (DeckInstance instance in deckInstances) {
            if (filteredCombatInstances.Contains(instance.combatInstance)) {
                filteredDeckInstances.Add(instance);
            }
        }
        document.map.AddItems<DeckInstance>(outputKey, filteredDeckInstances);
    }
}