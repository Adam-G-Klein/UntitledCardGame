using Unity.Services.Analytics;

public class CombatEndedAnalyticsEvent : BaseAnalyticsEvent
{
    public CombatEndedAnalyticsEvent() : base("combatEnded")
    {
    }

    public string EncounterName { set { SetParameter("encounterName", value); } }
    public int TurnIndex { set { SetParameter("turnIndex", value); } }
}
