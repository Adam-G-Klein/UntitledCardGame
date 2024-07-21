using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

public class UpgradeButtonTooltipProvider : MonoBehaviour
{
    private TooltipOnHover tooltipOnHover;

    public ShopDataSO shopData;

    public PlayerDataVariableSO playerData;
    void Start() {
        RefreshTooltip();
    }

    // Make it easy to refresh the tooltip, because we want to reload it after the
    // shop level changes.
    public void RefreshTooltip() {
        tooltipOnHover = GetComponent<TooltipOnHover>();
        tooltipOnHover.tooltip = new TooltipViewModel();
        ShopLevel level = shopData.GetShopLevel(playerData.GetValue().shopLevel);
        tooltipOnHover.tooltip += level.upgradeTooltip;
    }
}