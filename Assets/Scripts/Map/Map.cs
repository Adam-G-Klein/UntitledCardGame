using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public class Map
{
    [SerializeReference]
    public List<Encounter> encounters;

    public Map(List<Encounter> encounters) {
        this.encounters = encounters;
    }

    public Map(List<EncounterSerializable> encounters, SORegistry registry) {
        this.encounters = encounters.Select<EncounterSerializable, Encounter>(encounter => encounter.encounterType == EncounterType.Enemy ?
            new EnemyEncounter(encounter as EnemyEncounterSerializable, registry) : new ShopEncounter(encounter as ShopEncounterSerializable, registry)).ToList();
    }

    public void loadEncounterById(string id, GameStateVariableSO gameState) {
        Encounter encounterToLoad = getEncounterById(id);
        if (encounterToLoad == null) {
            // Make this actually do something
            // SceneManager.LoadScene("End");
            Debug.LogError("Cannot find encounter with given id " + id);
            return;
        }

        if(gameState.activeEncounter  == null) {
            Debug.LogError("Encounter variable is null, it's likely you need to create an empty " +
            "EncounterVariableSO and set it to activeEncounter or nextEncounter in the GameStateVariable");
            return;
        }
        gameState.nextEncounter.SetValue(encounterToLoad);
        switch (encounterToLoad.getEncounterType()) {
            case EncounterType.Enemy:
                gameState.LoadNextLocation(Location.TEAM_SELECT);
            break;

            case EncounterType.Shop:
                gameState.LoadNextLocation(Location.SHOP);
            break;

            default:
                // Also make this do something
                SceneManager.LoadScene("End");
            break;
        }
    }

    public Encounter getEncounterById(string id) {
        foreach (Encounter encounter in this.encounters) {
            if (encounter.id == id) {
                return encounter;
            }
        }

        return null;
    }
}