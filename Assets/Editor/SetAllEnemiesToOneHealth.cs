using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SetAllEnemiesToOneHealth {
    [MenuItem("Tools/Set All Enemies To One Health")]
    public static void SetToOne()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Game must be playing to modify scene instances.");
            return;
        }

        // Find all active EnemyInstance components in the loaded scene
        EnemyInstance[] enemies = UnityEngine.Object.FindObjectsOfType<EnemyInstance>();

        if (enemies.Length == 0)
        {
            Debug.Log("No EnemyInstance components found.");
            return;
        }

        foreach (var enemy in enemies)
        {
            enemy.combatInstance.combatStats.currentHealth = 1;
            EditorUtility.SetDirty(enemy);
        }
    }
}