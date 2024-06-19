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
public class CombatCompanionPortraitTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    [SerializeField]
    private CharacterPortrait portrait;
    void Start() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        if(!portrait) {
            Debug.LogError("No portrait attached to CombatCompanionPortraitTooltipProvider");
            return;
        }
        CompanionInstance companion = portrait.companionInstance;
        tooltipOnHover.tooltip += companion.companion.companionType.tooltip;

    }

}