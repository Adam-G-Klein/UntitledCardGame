using Unity.Services.Analytics;

public abstract class BaseAnalyticsEvent : Event
{
    protected BaseAnalyticsEvent(string eventName) : base(eventName)
    {
        // You can set any shared fields that should apply to all analytics events.
        AddCommonMetadata();
    }

    private void AddCommonMetadata()
    {
        // For example, the ID of the current run.
        SetParameter("runID", AnalyticsManager.Instance.gameState.currentRunID.ToString());

        // TODO: add more here, current version of the game, ascension level, etc.
        // SetParameter("playerLevel", PlayerStats.Level);
    }
}
