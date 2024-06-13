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


[RequireComponent(typeof(CharacterPortrait))]
[RequireComponent(typeof(TooltipOnHover))]
public class CombatCompanionPortraitTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    void Start() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new Tooltip();
        CompanionInstance companion = GetComponent<CharacterPortrait>().companionInstance;
        tooltipOnHover.tooltip += companion.companion.companionType.tooltip;

    }
    
}