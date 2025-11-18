using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Linq;

/*
Just grab the tooltiop from the companionType on the attached CompanionInstance

*/


[RequireComponent(typeof(TooltipOnHover))]
[RequireComponent(typeof(CompanionInstance))]
public class CombatCompanionTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    private Dictionary<StatusEffectType, TooltipViewModel> statusTooltips = new Dictionary<StatusEffectType, TooltipViewModel>();
    void Start()
    {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        CompanionInstance companion = GetComponent<CompanionInstance>();
        tooltipOnHover.tooltip += companion.companion.companionType.GetTooltip();
        foreach (PowerSO power in companion.combatInstance.GetUniquePowers())
        {
            tooltipOnHover.tooltip += power.GetTooltip();
        }
    }

    public void AddTooltip(TooltipViewModel tooltip)
    {
        tooltipOnHover.tooltip += tooltip;
    }

    public void DisableTooltip()
    {
        tooltipOnHover.Destroy();
    }

    public void UpdateStatusTooltips(Dictionary<StatusEffectType, int> statusMap, List<StatusEffect> statusEffects) {
        HashSet<StatusEffectType> statuses = new HashSet<StatusEffectType>();
        foreach (KeyValuePair<StatusEffectType, int> kvp in statusMap) {
            statuses.Add(kvp.Key);
        }

        foreach (StatusEffect status in statusEffects) {
            if (statuses.Contains(status.type) && !statusTooltips.ContainsKey(status.type)) {
                statusTooltips.Add(status.type, status.tooltip);
                tooltipOnHover.tooltip += status.tooltip;
            } else if (!statuses.Contains(status.type) && statusTooltips.ContainsKey(status.type)) {
                statusTooltips.Remove(status.type);
                tooltipOnHover.tooltip -= status.tooltip;
            }
        }        
    }
}