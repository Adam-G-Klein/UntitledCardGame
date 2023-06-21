using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountStep : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private List<TypeToCount> typesToCount = new List<TypeToCount>();
    [SerializeField]
    private string outputKey = "";

    public CountStep() {
        effectStepName = "CountStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        int outputInt = 0;
        foreach (TypeToCount type in typesToCount) {
            switch (type) {
                case TypeToCount.Card:
                    outputInt = outputInt + countCard(document);
                break;

                case TypeToCount.CardInHand:
                    outputInt = outputInt + countCardInHand(document);
                break;

                case TypeToCount.Companion:
                    outputInt = outputInt + countCompanions(document);
                break;

                case TypeToCount.Minion:
                    outputInt = outputInt + countMinions(document);
                break;

                case TypeToCount.Enemy:
                    outputInt = outputInt + countEnemies(document);
                break;
            }
        }
        document.intMap[outputKey] = outputInt;
        yield return null;
    }

    private int countCard(EffectDocument document) {
        if (!document.cardMap.containsValueWithKey(inputKey)) {
            EffectError("No input cards found under key " + inputKey);
            return 0;
        }

        List<Card> cards = document.cardMap.getList(inputKey);
        return cards.Count;
    }

    private int countCardInHand(EffectDocument document) {
        if (!document.playableCardMap.containsValueWithKey(inputKey)) {
            EffectError("No input PlayableCard for given key " + inputKey);
            return 0;
        }

        List<PlayableCard> cards = document.playableCardMap.getList(inputKey);
        return cards.Count;
    }

    private int countCompanions(EffectDocument document) {
        if (!document.companionMap.containsValueWithKey(inputKey)) {
            EffectError("No input companions for given key " + inputKey);
            return 0;
        }

        List<CompanionInstance> companions = document.companionMap.getList(inputKey);
        return companions.Count;
    }

    private int countMinions(EffectDocument document) {
        if (!document.minionMap.containsValueWithKey(inputKey)) {
            EffectError("No input minions for given key " + inputKey);
            return 0;
        }

        List<MinionInstance> minions = document.minionMap.getList(inputKey);
        return minions.Count;
    }

    private int countEnemies(EffectDocument document) {
        if (!document.enemyMap.containsValueWithKey(inputKey)) {
            EffectError("No input enemies for given key " + inputKey);
            return 0;
        }

        List<EnemyInstance> enemies = document.enemyMap.getList(inputKey);
        return enemies.Count;
    }

    public enum TypeToCount {
        Card,
        CardInHand,
        Companion,
        Minion,
        Enemy
    }
}