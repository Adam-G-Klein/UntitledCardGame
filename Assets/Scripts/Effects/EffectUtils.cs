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
        document.map.AddItem(key, companion);
        document.map.AddItem(key, companion.combatInstance);
        document.map.AddItem(key, companion.deckInstance);
    }

    public static void AddMinionsToDocument(
            EffectDocument document,
            string key,
            List<MinionInstance> minionInstances) {
        foreach (MinionInstance instance in minionInstances) {
            AddMinionToDocument(document, key, instance);
        }
    }

    public static void AddMinionToDocument(
            EffectDocument document,
            string key,
            MinionInstance minion) {
        document.map.AddItem(key, minion);
        document.map.AddItem(key, minion.combatInstance);
        document.map.AddItem(key, minion.deckInstance);
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
        document.map.AddItem(key, enemy);
        document.map.AddItem(key, enemy.combatInstance);
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
        document.map.AddItem(key, playableCard);
        document.map.AddItem(key, playableCard.card);
    }
}