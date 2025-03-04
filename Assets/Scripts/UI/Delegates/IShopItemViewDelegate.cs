using System;
using UnityEngine.UIElements;

public interface IShopItemViewDelegate {
    void ShopItemOnClick(ShopItemView shopItemView);
    void ShopItemViewHovered(ShopItemView shopItemView);
    void RerollButtonOnClick();
    void UpgradeButtonOnClick();

    void DisplayTooltip(VisualElement element, TooltipViewModel tooltipViewModel, bool forCompanionManagementView);
    void DestroyTooltip(VisualElement element);
}