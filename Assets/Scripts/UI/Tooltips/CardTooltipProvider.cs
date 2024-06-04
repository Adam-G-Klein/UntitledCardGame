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
        tooltipOnHover.tooltip = new Tooltip();
        PlayableCard card = GetComponent<PlayableCard>();
        if(card) {
            AddTooltipForPlayableCard(card);
        } 
        
    }

    private void AddTooltipForPlayableCard(PlayableCard card){
        CardType cardType = card.card.cardType;
        List<EffectWorkflow> effectWorkflows = cardType.effectWorkflows;
        Tooltip tooltip = null;
        Debug.Log("CardTooltipProvider: Found " + effectWorkflows.Count + " effect workflows");
        foreach(EffectWorkflow workflow in effectWorkflows) {
            foreach(EffectStep step in workflow.effectSteps) {
                Debug.Log("CardTooltipProvider: Found effect step " + step.effectStepName);
                if(step is ITooltipProvider) {
                    ITooltipProvider tooltipProvider = (ITooltipProvider) step;
                    tooltip = tooltipProvider.GetTooltip();
                    // + is overridden in Tooltip class to concatenate plaintext strings
                    // this code should stay operable when images are added if we update the
                    // operation override 
                    tooltipOnHover.tooltip += tooltip;
                    Debug.Log("CardTooltipProvider: Added tooltip " + tooltip.plainText);
                }
            }
        }
        tooltipOnHover.tooltip += cardType.GetTooltip();


    }
}