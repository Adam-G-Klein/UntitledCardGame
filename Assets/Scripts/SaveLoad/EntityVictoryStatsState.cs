using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class EntityVictoryStatsState
{
     private Dictionary<string, EntityVictoryStats> entityVictoryStatsDictionary = new();

     public EntityVictoryStatsState(Dictionary<string, EntityVictoryStats> entityVictoryStatsDictionary)
     {
          this.entityVictoryStatsDictionary = entityVictoryStatsDictionary;
     }

     public void LoadToLocalEntityVictoryState()
     {
          EntityVictoryStatsManager.Instance.entityVictoryStatsDictionary = entityVictoryStatsDictionary.ToDictionary(entry => entry.Key, entry => new EntityVictoryStats(entry.Value.runsRunWith, entry.Value.maxAscensionWonWith));

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