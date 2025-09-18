using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EffectUtils {
    public static void AddCompanionsToDocument(
            EffectDocument document,
            string key,
            List<CompanionInstance> companionInstances) {
        foreach (CompanionInstance instance in companionInstances) {
            AddCompanionToDocument(document, key, instance);
        }
    }

    public static void AddCompanionToDocument(
            EffectDocument document,
            string key,
            CompanionInstance companion) {
        string dedupeLink = companion.companion.id;
        document.map.AddItem(key, companion, dedupeLink);
        document.map.AddItem(key, companion.combatInstance, dedupeLink);
        document.map.AddItem(key, companion.deckInstance, dedupeLink);
        document.map.AddItem(key, companion.gameObject, dedupeLink);
        document.map.AddItem(key, companion.combatInstance.GetVisualElement(), dedupeLink);
    }

    public static void AddEnemiesToDocument(
            EffectDocument document,
            string key,
            List<EnemyInstance> enemyInstances) {
        foreach (EnemyInstance instance in enemyInstances) {
            AddEnemyToDocument(document, key, instance);
        }
    }

    public static void AddEnemyToDocument(
            EffectDocument document,
            string key,
            EnemyInstance enemy) {
        string dedupeLink = enemy.enemy.id;
        document.map.AddItem(key, enemy, dedupeLink);
        document.map.AddItem(key, enemy.combatInstance, dedupeLink);
        document.map.AddItem(key, enemy.gameObject, dedupeLink);
        document.map.AddItem(key, enemy.combatInstance.GetVisualElement(), dedupeLink);
    }

    public static void AddPlayableCardsToDocument(
            EffectDocument document,
            string key,
            List<PlayableCard> playableCards) {
        foreach (PlayableCard card in playableCards) {
            AddPlayableCardToDocument(document, key, card);
        }
    }

    public static void AddPlayableCardToDocument(
            EffectDocument document,
            string key,
            PlayableCard playableCard) {
        string dedupeLink = playableCard.card.id;
        document.map.AddItem(key, playableCard, dedupeLink);
        document.map.AddItem(key, playableCard.card, dedupeLink);
        document.map.AddItem(key, playableCard.gameObject, dedupeLink);
    }
}