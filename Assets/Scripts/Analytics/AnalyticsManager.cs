using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using Unity.Services.Analytics;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

[System.Serializable]
public class AnalyticsManager : GenericSingleton<AnalyticsManager>
{
    public GameStateVariableSO gameState;
    // This is so we only initialize the UnityGameServices once per session.
    // We do not want to keep initializing whenever we change scenes.
    public AnalyticsState state;

    public void RecordEvent(BaseAnalyticsEvent eventData)
    {
        Debug.Log("[AnalyticsManager]: recording event " + eventData.GetType().Name);
        AnalyticsService.Instance.RecordEvent(eventData);
    }

    async void Start()
    {
        await InitializeAnalyticsServices();
    }

    private async Task InitializeAnalyticsServices()
    {
        if (state.analyticsServicesInitialized)
        {
            return;
        }

        try
        {
            Debug.Log("[AnalyticsManager]: Initializing Unity Services...");
            // Please give me options for setting the Unity environment to "dev" or "prod" depending on the editor setting.
            // Right now, it always defaults to "prod" which is not what we want when testing.
            var options = new InitializationOptions();
            string envName;
#if UNITY_EDITOR
            envName = "dev";
#else
            envName = "production";
#endif
            Debug.Log("[AnalyticsManager]: Environment in use is " + envName);
            options.SetEnvironmentName(envName);
            await UnityServices.InitializeAsync(options);
            ConfigureDataCollection();

            state.analyticsServicesInitialized = true;
            Debug.Log("[AnalyticsManager]: Analytics initialized successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AnalyticsManager]: Initialization failed — {ex.Message}");
        }
    }

    public void ConfigureDataCollection()
    {
        // According to user consent setting, we start or stop collction.
        Debug.Log($"[AnalyticsManager]: Configuring data collection to value {gameState.consentToDataCollection}");
        if (gameState.consentToDataCollection)
        {
            AnalyticsService.Instance.StartDataCollection();
        }
        else
        {
            AnalyticsService.Instance.StopDataCollection();
        }
    }
}
