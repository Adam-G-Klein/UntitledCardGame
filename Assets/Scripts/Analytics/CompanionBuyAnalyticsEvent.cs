using Unity.Services.Analytics;

public class CompanionBuyAnalyticsEvent : BaseAnalyticsEvent
{
    public CompanionBuyAnalyticsEvent() : base("companionBought")
    {
    }

    public string CompanionName { set { SetParameter("companionName", value); } }
    public string PackName { set { SetParameter("packName", value); } }
    public string Rarity { set { SetParameter("rarity", value); } }
}
