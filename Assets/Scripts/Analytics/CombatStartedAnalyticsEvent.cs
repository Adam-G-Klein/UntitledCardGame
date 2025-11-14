using Unity.Services.Analytics;

public class CombatStartedAnalyticsEvent : BaseAnalyticsEvent
{
    public CombatStartedAnalyticsEvent() : base("combatStarted")
    {
    }

    public string EncounterName { set { SetParameter("encounterName", value); } }
}
