using Unity.Services.Analytics;


public class RunStartedAnalyticsEvent : BaseAnalyticsEvent
{
    public RunStartedAnalyticsEvent() : base("runStarted")
    {
    }

    // Record things such as the starting team or the ascensions.
}
