using System;
using UnityEngine.UIElements;

public interface IShopItemViewDelegate {
    void ShopItemOnClick(ShopItemView shopItemView);
    void ShopItemViewHovered(ShopItemView shopItemView);
    void RerollButtonOnClick(ClickEvent evt);
    void UpgradeButtonOnClick(ClickEvent evt);

    void DisplayTooltip(VisualElement element, TooltipViewModel tooltipViewModel, bool forCompanionManagementView);
    void DestroyTooltip(VisualElement element);
}