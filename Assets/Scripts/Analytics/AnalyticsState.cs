using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/AnalyticsState")]
public class AnalyticsState : ScriptableObject
{
    public bool analyticsServicesInitialized = false;

    private void OnEnable()
    {
        analyticsServicesInitialized = false;
    }
}
