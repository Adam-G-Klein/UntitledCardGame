using Unity.Services.Analytics;

public class CardBuyAnalyticsEvent : BaseAnalyticsEvent
{
    public CardBuyAnalyticsEvent() : base("cardBought")
    {
    }

    public string CardName { set { SetParameter("cardName", value); } }
    public string PackName { set { SetParameter("packName", value); } }
    public string CompanionName { set { SetParameter("companionName", value); } }
}
