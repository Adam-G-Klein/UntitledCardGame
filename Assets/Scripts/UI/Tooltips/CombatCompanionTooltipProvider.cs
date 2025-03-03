using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

/*
Just grab the tooltiop from the companionType on the attached CompanionInstance

*/


[RequireComponent(typeof(TooltipOnHover))]
[RequireComponent(typeof(CompanionInstance))]
public class CombatCompanionTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    void Start() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        CompanionInstance companion = GetComponent<CompanionInstance>();
        tooltipOnHover.tooltip += companion.companion.companionType.tooltip;
    }

    public void DisableTooltip() {
        tooltipOnHover.Destroy();
    }
}