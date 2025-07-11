using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class EntityVictoryStatsManager : GenericSingleton<EntityVictoryStatsManager>
{
    public Dictionary<string, EntityVictoryStats> entityVictoryStatsDictionary = new();

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ReportWin(List<Companion> companions, int ascensionLevel)
    {
        Debug.Log(entityVictoryStatsDictionary.Count + " entities in victory stats dictionary before reporting win.");
        Debug.Log($"Reporting win for ascension level {ascensionLevel} with {companions.Count} companions.");
        // If you have multiple instances of a companion or card it will count each one separately. 
        // Easy to change if we don't want to double count, I just wasn't sure which made more sense here.
        // This is most prevalent for things like strike/defend which are likely to show up very often.
        foreach (Companion companion in companions)
        {
            UpdateVictoryStatsForEntity(companion.companionType.name, companion.companionType.GUID, ascensionLevel);
            foreach (Card card in companion.deck.cards)
            {
                UpdateVictoryStatsForEntity(card.cardType.name, card.cardType.GUID, ascensionLevel);
            }
        }
        SaveManager.Instance.SaveEntityVictoryStats();
    }
 
    private void UpdateVictoryStatsForEntity(string name, string GUID, int ascensionLevel)
    {
        if (entityVictoryStatsDictionary.ContainsKey(GUID))
        {
            entityVictoryStatsDictionary[GUID].runsRunWith++;
            entityVictoryStatsDictionary[GUID].maxAscensionWonWith = Math.Max(entityVictoryStatsDictionary[GUID].maxAscensionWonWith, ascensionLevel);
        }
        else
        {
            entityVictoryStatsDictionary.Add(GUID, new EntityVictoryStats(1, ascensionLevel));
        }
        Debug.Log($"Updated victory stats for {name} ({GUID}): Runs - {entityVictoryStatsDictionary[GUID].runsRunWith}, Max Ascension - {entityVictoryStatsDictionary[GUID].maxAscensionWonWith}");
    }
}
