using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SaveState {

    // private List<CompanionSerializeable> activeCompanions;
    // private List<CompanionSerializeable> benchCompanions;
    // private List<EncounterSerializeable> map;
    // private int currentEncounterIndex;
    // private PlayerDataSerializeable playerData;

    public SaveState(string saveName, GameStateVariableSO gameState) {
        // this.activeCompanions = gameState.companions.activeCompanions
        //     .Select(companion => new CompanionSerializeable(companion))
        //     .ToList();
        
        // this.benchCompanions = gameState.companions.benchedCompanions
        //     .Select(companion => new CompanionSerializeable(companion))
        //     .ToList();
        
        // this.map = gameState.map.GetValue().encounters
        //     .Select(encounter => new EncounterSerializeable(encounter))
        //     .ToList();
        
        // this.currentEncounterIndex = gameState.currentEncounterIndex;

        // this.playerData = new PlayerDataSerializeable(gameState.playerData);
    }

    public void LoadToGameState(GameStateVariableSO gameState, SORegistry registry) {
        // gameState.companions.activeCompanions = this.activeCompanions
        //     .Select(companion => new Companion(companion, registry))
        //     .ToList();
        
        // gameState.companions.benchedCompanions = this.benchCompanions
        //     .Select(companion => new Companion(companion, registry))
        //     .ToList();
        
        // gameState.map.SetValue(new Map(this.map, registry));

        // gameState.currentEncounterIndex = this.currentEncounterIndex;

        // gameState.playerData.SetValue(new PlayerData(this.playerData));
    }
}