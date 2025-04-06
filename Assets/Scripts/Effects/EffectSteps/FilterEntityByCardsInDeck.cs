using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterEntityByCardsInDeck : EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private CardFilter filter;
    [SerializeField]
    private int threshold = 5;
    [SerializeField]
    private string outputKey = "";

    public FilterEntityByCardsInDeck() {
        effectStepName = "FilterEntityByCardsInDeck";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CompanionInstance> companionInstances = document.map.GetList<CompanionInstance>(inputKey);
        if (companionInstances.Count == 0) {
            EffectError("No input targets present for key " + inputKey);
            yield return null;
        }
        List<CombatInstance> filteredList = new List<CombatInstance>();
        for (int i = 0; i < companionInstances.Count; i++) {
            string tmpKey = "tmpCompanion" + i;
            EffectUtils.AddCompanionToDocument(document, tmpKey, companionInstances[i]);
            CountCardsInDeck countStep = new CountCardsInDeck(tmpKey, filter, "tmpCount");
            yield return countStep.invoke(document);
            int count = document.intMap["tmpCount"];
            Debug.Log("Companion " + i + " has " + count + " cards in their deck matching the filter");
            if (count >= threshold) {
                filteredList.Add(companionInstances[i].combatInstance);
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
        document.map.AddItems<CompanionInstance>(outputKey, filteredCompanionInstances, "numCardsLink");
    }

    private void FilterEnemyInstances(
            EffectDocument document,
            List<CombatInstance> filteredCombatInstances) {
        List<EnemyInstance> enemyInstances = document.map.TryGetList<EnemyInstance>(inputKey);
        List<EnemyInstance> filteredEnemyInstances = new List<EnemyInstance>();
        foreach (EnemyInstance instance in enemyInstances) {
            if (filteredCombatInstances.Contains(instance.combatInstance)) {
                filteredEnemyInstances.Add(instance);
            }
        }
        document.map.AddItems<EnemyInstance>(outputKey, filteredEnemyInstances, "numCardsLink");
    }

    private void FilterDeckInstances(
            EffectDocument document,
            List<CombatInstance> filteredCombatInstances) {
        List<DeckInstance> deckInstances = document.map.TryGetList<DeckInstance>(inputKey);
        List<DeckInstance> filteredDeckInstances = new List<DeckInstance>();
        foreach (DeckInstance instance in deckInstances) {
            if (filteredCombatInstances.Contains(instance.combatInstance)) {
                filteredDeckInstances.Add(instance);
            }
        }
        document.map.AddItems<DeckInstance>(outputKey, filteredDeckInstances, "numCardsLink");
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}
