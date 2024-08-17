using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;
using System;

/*
Just grab the tooltiop from the enemyType on the attached EnemyInstance

*/


[RequireComponent(typeof(TooltipOnHover))]
[RequireComponent(typeof(EnemyInstance))]
public class CombatEnemyTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    private int nextBehaviorIndex;
    private EnemyInstance enemy;
    void Start() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        enemy = GetComponent<EnemyInstance>();
        nextBehaviorIndex = enemy.enemy.enemyType.enemyPattern.nextBehaviorIndex;
        UpdateToolTip();
    }

    void Update() {
        int newIndex = enemy.enemy.enemyType.enemyPattern.nextBehaviorIndex;
        if (nextBehaviorIndex != newIndex) {
            nextBehaviorIndex = newIndex;
            UpdateToolTip();
        }
    }

    void UpdateToolTip() {
        Debug.Log("nextBehaviorIndex");
        Debug.Log(nextBehaviorIndex);
        List<TooltipLine> lines = new List<TooltipLine>();
        List<TooltipLine> allLines = enemy.enemy.enemyType.tooltip.lines;
        foreach(TooltipLine line in allLines) {
            if ((nextBehaviorIndex == line.relatedBehaviorIndex) || (line.relatedBehaviorIndex == -1)) {
                lines.Add(line);
            }
        }
        TooltipViewModel tempViewModel = new TooltipViewModel(lines);
        tooltipOnHover.tooltip.empty = true; // pseudo reset of the current tooltip.
        tooltipOnHover.tooltip += tempViewModel;
    }
}