using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Map
{
    public List<EncounterReference> encounters;

    public void loadEncounterById(string id, EncounterVariableSO encounterVariable) {
        Encounter encounterToLoad = getEncounterById(id);
        if (encounterToLoad == null) {
            // Make this actually do something
            // SceneManager.LoadScene("End");
            Debug.LogError("Cannot find encounter with given id " + id);
            return;
        }

        encounterVariable.SetValue(encounterToLoad);
        switch (encounterToLoad.getEncounterType()) {
            case EncounterType.Enemy:
                SceneManager.LoadScene("PlaceholderEnemyEncounter");
            break;

            case EncounterType.Shop:
                SceneManager.LoadScene("PlaceholderShopEncounter");
            break;

            default:
                // Also make this do something
                SceneManager.LoadScene("End");
            break;
        }
    }

    public void loadMapScene() {
        SceneManager.LoadScene("Map");
    }

    public void loadNextEncounter(EncounterVariableSO activeEncounterVariable) {
        Encounter nextEncounter = getNextEncounter(activeEncounterVariable.GetValue());
        if (nextEncounter == null) {
            // Make this actually do something
            SceneManager.LoadScene("End");
        }
        activeEncounterVariable.SetValue(nextEncounter);
        switch (nextEncounter.getEncounterType()) {
            case EncounterType.Enemy:
                SceneManager.LoadScene("PlaceholderEnemyEncounter");
            break;

            case EncounterType.Shop:
                SceneManager.LoadScene("PlaceholderShopEncounter");
            break;

            default:
                // Also make this do something
                SceneManager.LoadScene("End");
            break;
        }
    }

    private Encounter getNextEncounter(Encounter activeEncounter) {
        for(int i = 0; i < encounters.Count; i++) {
            if (encounters[i].Value.id == activeEncounter.id) {
                if (i+1 == encounters.Count) {
                    return null;
                }
                return encounters[i+1].Value;
            }
        }
        Debug.LogError("Active encounter not found in encounters list");
        return null;
    }

    private Encounter getEncounterById(string id) {
        foreach (EncounterReference encounterReference in this.encounters) {
            if (encounterReference.Value.id == id) {
                return encounterReference.Value;
            }
        }

        return null;
    }
}
