using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;


[System.Serializable]
public class EntityVictoryStatsState
{
     private Dictionary<string, EntityVictoryStats> entityVictoryStatsDictionary = new();
     [OptionalField]
     private int ascensionIndexVersion;

     public EntityVictoryStatsState(Dictionary<string, EntityVictoryStats> entityVictoryStatsDictionary)
     {
          this.entityVictoryStatsDictionary = entityVictoryStatsDictionary;
          this.ascensionIndexVersion = AscensionInfo.AscensionIndexVersion;
     }

     public void LoadToLocalEntityVictoryState()
     {
          int ascensionOffset = ascensionIndexVersion < AscensionInfo.AscensionIndexVersion ? 1 : 0;
          EntityVictoryStatsManager.Instance.entityVictoryStatsDictionary = entityVictoryStatsDictionary.ToDictionary(entry => entry.Key, entry => new EntityVictoryStats(entry.Value.runsRunWith, entry.Value.maxAscensionWonWith + ascensionOffset));

          // PrintForDebugging(); // Uncomment for debugging purposes
     }

     private void PrintForDebugging()
     {
          foreach (var kvp in entityVictoryStatsDictionary)
          {
               Debug.Log($"Entity: {kvp.Key}, Runs: {kvp.Value.runsRunWith}, Max Ascension: {kvp.Value.maxAscensionWonWith}");
          }  
     }
}

[System.Serializable]
public class EntityVictoryStats
{
     public int runsRunWith;
     public int maxAscensionWonWith;

     public EntityVictoryStats(int runsRunWith, int maxAscensionWonWith)
     {
          this.runsRunWith = runsRunWith;
          this.maxAscensionWonWith = maxAscensionWonWith;
     }
}