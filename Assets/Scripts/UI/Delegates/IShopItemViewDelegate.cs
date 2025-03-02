using System;

public interface IShopItemViewDelegate {
    void ShopItemOnClick(ShopItemView shopItemView);
    void ShopItemViewHovered(ShopItemView shopItemView);
    void RerollButtonOnClick();
    void UpgradeButtonOnClick();
}