using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using UnityEngine.Rendering;

/*
Just grab the tooltiop from the enemyType on the attached EnemyInstance

*/


[RequireComponent(typeof(TooltipOnHover))]
[RequireComponent(typeof(EnemyInstance))]
public class CombatEnemyTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    private int behaviorIndex;
    private EnemyInstance enemy;

    private Dictionary<StatusEffectType, TooltipViewModel> statusTooltips = new Dictionary<StatusEffectType, TooltipViewModel>();

    internal void DisableTooltip()
    {
        tooltipOnHover.Destroy();
    }

    void Start() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        enemy = GetComponent<EnemyInstance>();
        behaviorIndex = enemy.GetBehaviorIndexForBrain(enemy.enemy.enemyType.enemyPattern);
        UpdateToolTip();
    }

    public void IntentDeclared(int behaviorIndex) {
        this.behaviorIndex = behaviorIndex;
        UpdateToolTip();
    }

    void UpdateToolTip() {
        Debug.Log("behaviorIndex");
        Debug.Log(behaviorIndex);
        List<TooltipLine> lines = new List<TooltipLine>();
        List<TooltipLine> allLines = enemy.enemy.enemyType.tooltip.lines;
        foreach(TooltipLine line in allLines) {
            if ((behaviorIndex == line.relatedBehaviorIndex) || (line.relatedBehaviorIndex == -1)) {
                lines.Add(line);
            }
        }
        TooltipViewModel tempViewModel = new TooltipViewModel(lines);
        tooltipOnHover.tooltip.empty = true; // pseudo reset of the current tooltip.
        tooltipOnHover.tooltip += tempViewModel;

        foreach (KeyValuePair<StatusEffectType, TooltipViewModel> kvp in statusTooltips) {
            tooltipOnHover.tooltip += kvp.Value;
        }
    }

    public void UpdateStatusTooltips(Dictionary<StatusEffectType, int> statusMap, List<StatusEffect> statusEffects) {
        HashSet<StatusEffectType> statuses = new HashSet<StatusEffectType>();
        foreach (KeyValuePair<StatusEffectType, int> kvp in statusMap) {
            statuses.Add(kvp.Key);
        }

        foreach (StatusEffect status in statusEffects) {
            if (statuses.Contains(status.type) && !statusTooltips.ContainsKey(status.type)) {
                statusTooltips.Add(status.type, status.tooltip);
            } else if (!statuses.Contains(status.type) && statusTooltips.ContainsKey(status.type)) {
                statusTooltips.Remove(status.type);
            }
        }        
    }
}