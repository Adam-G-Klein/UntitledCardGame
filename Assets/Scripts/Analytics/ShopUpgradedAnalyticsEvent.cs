using Unity.Services.Analytics;

public class ShopUpgradedAnalyticsEvent : BaseAnalyticsEvent
{
    public ShopUpgradedAnalyticsEvent() : base("shopUpgraded")
    {
    }

    public int ShopLevel { set { SetParameter("shopLevel", value); } }
}
