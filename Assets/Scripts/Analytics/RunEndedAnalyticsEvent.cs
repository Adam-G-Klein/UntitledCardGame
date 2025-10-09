using Unity.Services.Analytics;

// RunEndedCondition encodes the way a player's run ends.
// Please keep the values stable because these are the values as they appear in SQL.
public enum RunEndedCondition
{
    VICTORY,
    DEFEAT,
}

public class RunEndedAnalyticsEvent : BaseAnalyticsEvent
{
    public RunEndedAnalyticsEvent() : base("runEnded")
    {
    }

    public RunEndedCondition EndCondition { set { SetParameter("runEndCondition", value.ToString()); } }
}
