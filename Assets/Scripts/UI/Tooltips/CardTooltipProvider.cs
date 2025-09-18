using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

/*
check through PlayableCard.cardType for different effects
if effect implements ITooltipProvider, use the tooltip
provide the tooltip to the attached toolTipOnHoverComponent

*/


public class CardTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    void Start() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        PlayableCard card = GetComponent<PlayableCard>();
        if(card) {
            tooltipOnHover.tooltip += card.card.cardType.GetTooltip();
        }
    }
}