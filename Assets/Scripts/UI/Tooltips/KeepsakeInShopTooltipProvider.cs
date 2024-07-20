using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

public class KeepsakeInShopTooltipProvder : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;
    void Start() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        KeepsakeInShop keepsake = GetComponent<KeepsakeInShop>();
        if(keepsake) {
            AddTooltipFromKeepsake(keepsake);
        } 
        
    }

    // Add 

    private void AddTooltipFromKeepsake(KeepsakeInShop keepsake){
        tooltipOnHover.tooltip += keepsake.companion.companionType.tooltip;
    }


}