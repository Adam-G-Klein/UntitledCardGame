using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class SaveState {

    private string saveName;
    public string SaveName => saveName;
    private List<CompanionSerializable> activeCompanions;
    private List<CompanionSerializable> benchCompanions;
    private List<EncounterSerializable> map;
    private int currentEncounterIndex;
    private PlayerDataSerializable playerData;

    public SaveState(string saveName, GameStateVariableSO gameState) {
        this.saveName = saveName;
        this.activeCompanions = gameState.companions.activeCompanions
             .Select(companion => new CompanionSerializable(companion))
             .ToList();

        this.benchCompanions = gameState.companions.benchedCompanions
             .Select(companion => new CompanionSerializable(companion))
             .ToList();

        this.map = gameState.map.GetValue().encounters
             .Select<Encounter, EncounterSerializable>(encounter => encounter.getEncounterType() == EncounterType.Enemy ?
                new EnemyEncounterSerializable((EnemyEncounter)encounter)
                : new ShopEncounterSerializable((ShopEncounter)encounter))
             .ToList();
        
        this.currentEncounterIndex = gameState.currentEncounterIndex;

        this.playerData = new PlayerDataSerializable(gameState.playerData.GetValue());
    }

    public void LoadToGameState(GameStateVariableSO gameState, SORegistry registry) {
        gameState.companions.activeCompanions = this.activeCompanions
             .Select(companion => new Companion(companion, registry))
             .ToList();
        
        gameState.companions.benchedCompanions = this.benchCompanions
             .Select(companion => new Companion(companion, registry))
             .ToList();
        
        gameState.map.SetValue(new Map(this.map, registry, gameState.baseShopData));

        gameState.currentEncounterIndex = this.currentEncounterIndex;

        gameState.playerData.SetValue(new PlayerData(this.playerData));
    }
}