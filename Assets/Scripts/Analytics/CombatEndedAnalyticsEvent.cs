using Unity.Services.Analytics;

public class CombatEndedAnalyticsEvent : BaseAnalyticsEvent
{
    public CombatEndedAnalyticsEvent() : base("combatEnded")
    {
    }
}
